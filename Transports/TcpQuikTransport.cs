// Copyright (c) 2026 Your Name / QuikSharp Community
// Licensed under the Apache License, Version 2.0

using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace QuikSharp.Transports
{
    /// <summary>
    /// Транспорт на основе двух TCP-соединений (request/response + callback), как в исходном
    /// "netstack" QuikSharp. Lua-скрипт выступает TCP-сервером (см. lua/nettransport.lua),
    /// этот класс — TCP-клиентом. Сообщения — JSON с разделителем "\n".
    /// Используется как альтернатива <see cref="ShmQuikTransport"/>, когда семафоры shared
    /// memory приводят к подвешиванию QUIK.
    /// </summary>
    public class TcpQuikTransport : IQuikTransport
    {
        private readonly string _host;
        private readonly int _responsePort;
        private readonly int _callbackPort;
        private readonly TimeSpan _connectTimeout;

        private TcpClient? _responseClient, _callbackClient;
        private StreamReader? _responseReader, _callbackReader;
        private StreamWriter? _responseWriter;
        private readonly SemaphoreSlim _writeLock = new(1, 1);

        private readonly ConcurrentDictionary<long, TaskCompletionSource<Message>> _pending = new();

        private CancellationTokenSource _cts = new();
        private Task? _responseTask;
        private Task? _callbackTask;

        private volatile bool _running;
        private long _nextRequestId = 0;

        private readonly JsonSerializerOptions _jsonOpts;

        public event Action<Candle>? OnNewCandle;
        public event Action<Order>? OnOrder;
        public event Action<Trade>? OnTrade;
        public event Action<TransactionReply>? OnTransReply;
        public event Action<StopOrder>? OnStopOrder;
        public event Action<AllTrade>? OnAllTrade;
        public event Action<OrderBook>? OnQuote;
        public event Action<Param>? OnParam;
        public event Action<AccountBalance>? OnAccountBalance;
        public event Action<AccountPosition>? OnAccountPosition;
        public event Action<DepoLimitEx>? OnDepoLimit;
        public event Action<DepoLimitDelete>? OnDepoLimitDelete;
        public event Action<Firm>? OnFirm;
        public event Action<FuturesClientHolding>? OnFuturesClientHolding;
        public event Action<FuturesLimits>? OnFuturesLimitChange;
        public event Action<FuturesLimitDelete>? OnFuturesLimitDelete;
        public event Action<MoneyLimitEx>? OnMoneyLimit;
        public event Action<MoneyLimitDelete>? OnMoneyLimitDelete;
        public event Action? OnConnected;
        public event Action? OnDisconnected;
        public event Action<string>? OnUnknownCallback;
        public event Action<Exception>? OnTransportError;

        private long _correlationId = 0;
        private int _transactionId = 0;
        private readonly object _transIdLock = new object();

        public bool IsConnected => _running;

        public TcpQuikTransport(
            string host = "127.0.0.1",
            int responsePort = 34130,
            int? callbackPort = null,
            JsonSerializerOptions? jsonOpts = null,
            TimeSpan? connectTimeout = null)
        {
            _host = host;
            _responsePort = responsePort;
            _callbackPort = callbackPort ?? responsePort + 1;
            _jsonOpts = jsonOpts ?? QuikJson.Options;
            _connectTimeout = connectTimeout ?? TimeSpan.FromSeconds(30);
        }

        internal long GetNewUniqueId()
        {
            var newId = Interlocked.Increment(ref _correlationId);

            if (newId > 0)
                return newId;

            Interlocked.Exchange(ref _correlationId, 1);
            return 1;
        }

        internal int GetUniqueTransactionId()
        {
            lock (_transIdLock)
            {
                if (_transactionId == 0)
                {
                    _transactionId = Convert.ToInt32(DateTime.Now.ToString("ddHHmmss"));

                    if (_transactionId < 100)
                        _transactionId = 100000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
                }
                else
                {
                    _transactionId++;

                    if (_transactionId >= 2_147_483_000)
                    {
                        _transactionId = 100;
                    }
                }

                return _transactionId;
            }
        }

        public async Task ConnectAsync(CancellationToken ct = default)
        {
            if (_running) return;

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(_connectTimeout);

            try
            {
                _responseClient = await ConnectWithRetryAsync(_responsePort, timeoutCts.Token).ConfigureAwait(false);
                _callbackClient = await ConnectWithRetryAsync(_callbackPort, timeoutCts.Token).ConfigureAwait(false);

                _responseClient.NoDelay = true;
                _callbackClient.NoDelay = true;

                var responseStream = _responseClient.GetStream();
                var callbackStream = _callbackClient.GetStream();

                _responseReader = new StreamReader(responseStream, Encoding.UTF8);
                _responseWriter = new StreamWriter(responseStream, Encoding.UTF8) { AutoFlush = true, NewLine = "\n" };
                _callbackReader = new StreamReader(callbackStream, Encoding.UTF8);

                _running = true;

                _responseTask = Task.Run(() => ResponseLoopAsync(_cts.Token));
                _callbackTask = Task.Run(() => CallbackLoopAsync(_cts.Token));

                OnConnected?.Invoke();
            }
            catch (Exception ex)
            {
                Dispose();
                OnTransportError?.Invoke(ex);
                throw;
            }
        }

        private async Task<TcpClient> ConnectWithRetryAsync(int port, CancellationToken ct)
        {
            Exception? lastError = null;

            while (!ct.IsCancellationRequested)
            {
                var client = new TcpClient();
                try
                {
                    await client.ConnectAsync(_host, port, ct).ConfigureAwait(false);
                    return client;
                }
                catch (Exception ex) when (ex is SocketException or OperationCanceledException)
                {
                    client.Dispose();
                    lastError = ex;

                    if (ex is OperationCanceledException)
                        break;

                    try
                    {
                        await Task.Delay(500, ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }

            ct.ThrowIfCancellationRequested();
            throw new TimeoutException($"Could not connect to Lua netstack server on port {port}", lastError);
        }

        public async Task<long> SendTransaction(Transaction transaction)
        {
            if (!transaction.TRANS_ID.HasValue || transaction.TRANS_ID.Value == 0)
            {
                transaction.TRANS_ID = GetUniqueTransactionId();
            }

            if (string.IsNullOrWhiteSpace(transaction.CLIENT_CODE))
                transaction.CLIENT_CODE = transaction.TRANS_ID.Value.ToString();

            try
            {
                var success = await SendAsync<Message, bool>(
                    new Message(transaction, "sendTransaction"), "sendTransaction")
                    .ConfigureAwait(false);

                return success ? transaction.TRANS_ID.Value : -transaction.TRANS_ID.Value;
            }
            catch (Exception ex)
            {
                transaction.ErrorMessage = ex.Message;
                return -transaction.TRANS_ID.Value;
            }
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(
            TRequest request,
            string command,
            CancellationToken ct = default)
        {
            if (!_running) throw new InvalidOperationException("_transport not running");

            long reqId = Interlocked.Increment(ref _nextRequestId);
            var msg = new Message
            {
                Id = reqId,
                cmd = command,
                Data = request,
                CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            string json = JsonSerializer.Serialize(msg, _jsonOpts);

            var tcs = new TaskCompletionSource<Message>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[reqId] = tcs;

            try
            {
                await _writeLock.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    await _responseWriter!.WriteLineAsync(json).ConfigureAwait(false);
                }
                finally
                {
                    _writeLock.Release();
                }

                var responseTask = tcs.Task;

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(45));

                var winner = await Task.WhenAny(responseTask, Task.Delay(Timeout.Infinite, cts.Token));

                if (winner != responseTask)
                {
                    _pending.TryRemove(reqId, out _);
                    throw new TimeoutException($"Request {reqId} ({command}) timed out after 45s");
                }

                var response = await responseTask;

                if (!string.IsNullOrEmpty(response.LuaError))
                    throw new Exception($"Lua error: {response.LuaError}");

                if (response.Data is JsonElement je)
                    return je.Deserialize<TResponse>(_jsonOpts)!;

                return JsonSerializer.Deserialize<TResponse>(
                    JsonSerializer.Serialize(response.Data, _jsonOpts), _jsonOpts)!;
            }
            catch
            {
                _pending.TryRemove(reqId, out _);
                throw;
            }
        }

        private async Task ResponseLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _running)
            {
                try
                {
                    var line = await _responseReader!.ReadLineAsync(ct).ConfigureAwait(false);
                    if (line == null)
                    {
                        // Собеседник закрыл соединение
                        _running = false;
                        OnDisconnected?.Invoke();
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(line)) continue;

                    Message? msg;
                    try
                    {
                        msg = JsonSerializer.Deserialize<Message>(line, _jsonOpts);
                    }
                    catch (JsonException ex)
                    {
                        OnTransportError?.Invoke(ex);
                        continue;
                    }

                    if (msg == null) continue;

                    if (!string.IsNullOrEmpty(msg.LuaError))
                    {
                        if (_pending.TryRemove(msg.Id ?? 0, out var errTcs))
                            errTcs.TrySetException(new Exception($"Lua error: {msg.LuaError}"));
                        continue;
                    }

                    if (_pending.TryRemove((long)msg.Id!, out var tcs))
                    {
                        tcs.TrySetResult(msg);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnTransportError?.Invoke(ex);
                    if (!_running) break;
                    await Task.Delay(300, ct).ConfigureAwait(false);
                }
            }
        }

        private async Task CallbackLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _running)
            {
                try
                {
                    var line = await _callbackReader!.ReadLineAsync(ct).ConfigureAwait(false);
                    if (line == null)
                    {
                        _running = false;
                        OnDisconnected?.Invoke();
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(line)) continue;

                    Message? msg;
                    try
                    {
                        msg = JsonSerializer.Deserialize<Message>(line, _jsonOpts);
                    }
                    catch (JsonException ex)
                    {
                        OnTransportError?.Invoke(ex);
                        continue;
                    }

                    if (msg == null) continue;

                    DispatchCallback(msg);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnTransportError?.Invoke(ex);
                    if (!_running) break;
                    await Task.Delay(500, ct).ConfigureAwait(false);
                }
            }
        }

        private void DispatchCallback(Message msg)
        {
            try
            {
                switch (msg.cmd?.ToLowerInvariant())
                {
                    case "newcandle": OnNewCandle?.Invoke(msg.GetData<Candle>()); break;
                    case "onorder": OnOrder?.Invoke(msg.GetData<Order>()); break;
                    case "ontrade": OnTrade?.Invoke(msg.GetData<Trade>()); break;
                    case "ontransreply": OnTransReply?.Invoke(msg.GetData<TransactionReply>()); break;
                    case "onstoporder": OnStopOrder?.Invoke(msg.GetData<StopOrder>()); break;
                    case "onalltrade": OnAllTrade?.Invoke(msg.GetData<AllTrade>()); break;
                    case "onquote": OnQuote?.Invoke(msg.GetData<OrderBook>()); break;
                    case "onparam": OnParam?.Invoke(msg.GetData<Param>()); break;
                    case "onaccountbalance": OnAccountBalance?.Invoke(msg.GetData<AccountBalance>()); break;
                    case "onaccountposition": OnAccountPosition?.Invoke(msg.GetData<AccountPosition>()); break;
                    case "ondepolimit": OnDepoLimit?.Invoke(msg.GetData<DepoLimitEx>()); break;
                    case "ondepolimitdelete": OnDepoLimitDelete?.Invoke(msg.GetData<DepoLimitDelete>()); break;
                    case "onfirm": OnFirm?.Invoke(msg.GetData<Firm>()); break;
                    case "onfuturesclientholding": OnFuturesClientHolding?.Invoke(msg.GetData<FuturesClientHolding>()); break;
                    case "onfutureslimitchange": OnFuturesLimitChange?.Invoke(msg.GetData<FuturesLimits>()); break;
                    case "onfutureslimitdelete": OnFuturesLimitDelete?.Invoke(msg.GetData<FuturesLimitDelete>()); break;
                    case "onmoneylimit": OnMoneyLimit?.Invoke(msg.GetData<MoneyLimitEx>()); break;
                    case "onmoneylimitdelete": OnMoneyLimitDelete?.Invoke(msg.GetData<MoneyLimitDelete>()); break;

                    default:
                        OnUnknownCallback?.Invoke(msg.cmd ?? "unknown");
                        break;
                }
            }
            catch (Exception ex)
            {
                OnTransportError?.Invoke(ex);
            }
        }

        public void Dispose()
        {
            if (!_running && _responseClient == null && _callbackClient == null) return;
            _running = false;

            try { _cts.Cancel(); } catch { }

            _responseTask?.Wait(1200);
            _callbackTask?.Wait(1200);

            SafeDispose(_responseReader); SafeDispose(_responseWriter); SafeDispose(_callbackReader);
            SafeDispose(_responseClient); SafeDispose(_callbackClient);
            SafeDispose(_writeLock);

            static void SafeDispose(IDisposable? d)
            {
                try { d?.Dispose(); } catch { }
            }
        }
    }
}
