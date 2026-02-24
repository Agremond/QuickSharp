    using System;
    using System.Buffers.Binary;
    using System.Collections.Concurrent;
    using System.IO;
    using System.IO.MemoryMappedFiles;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using QuikSharp.DataStructures;
    using QuikSharp.DataStructures.Transaction;

    namespace QuikSharp.Transports
    {
        /// <summary>
        /// Транспорт на основе Shared Memory + Semaphore + Mutex (вариант с отдельными буферами)
        /// Совместим с Lua-скриптом QUIK# (отдельные буферы для request/response/callback)
        /// </summary>
        public class ShmQuikTransport : IQuikTransport
        {
            // Имена должны совпадать с Lua-скриптом на 100%
            private const string REQ_SHM = "QuikSharp_Request_Shmem";
            private const string RESP_SHM = "QuikSharp_Response_Shmem";
            private const string CB_SHM = "QuikSharp_Callback_Shmem";

            private const string REQ_SEM = "QuikSharp_Request_Sem";
            private const string RESP_SEM = "QuikSharp_Response_Sem";
            private const string CB_SEM = "QuikSharp_Callback_Sem";

            private const string REQ_MTX = "QuikSharp_Request_MutexSem";
            private const string RESP_MTX = "QuikSharp_Response_MutexSem";
            private const string CB_MTX = "QuikSharp_Callback_MutexSem";

            private const uint MAGIC = 0x5155494B; // "QUIK"
            private const uint VERSION = 2;
            private const int HEADER = 24;         // 6 × uint32

            private const int REQ_SIZE = 1 * 1024 * 1024;
            private const int RESP_SIZE = 1 * 1024 * 1024;
            private const int CB_SIZE = 2 * 1024 * 1024;

            private const uint MSG_TYPE_REQUEST = 1;
            private const uint MSG_TYPE_RESPONSE = 2;
            public event Action<TransactionReply>? OnTransReply;
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
            // Ресурсы shared memory
            private MemoryMappedFile? _mmfReq, _mmfResp, _mmfCb;
            private MemoryMappedViewAccessor? _viewReq, _viewResp, _viewCb;

            private Semaphore? _semReq, _semResp, _semCb;
            private Semaphore? _mtxReq, _mtxResp, _mtxCb;

            // Очереди и состояния
            private readonly ConcurrentDictionary<long, TaskCompletionSource<Message>> _pending =
                new();

            private CancellationTokenSource _cts = new();
            private Task? _callbackTask;
            private Task? _responseTask;

            private volatile bool _running;
            private long _nextRequestId = 0;

            private readonly JsonSerializerOptions _jsonOpts;

            // События коллбэков (как в интерфейсе)
            public event Action<Candle>? OnNewCandle;
            public event Action<Order>? OnOrder;
            public event Action<Trade>? OnTrade;

            public event Action<StopOrder>? OnStopOrder;
            public event Action<AllTrade>? OnAllTrade;
            // Добавьте остальные по необходимости: OnParam, OnDepoLimit и т.д.

            public event Action<string>? OnUnknownCallback;
            public event Action<Exception>? OnTransportError;

            public bool IsConnected => _running;

            public ShmQuikTransport(JsonSerializerOptions? jsonOpts = null)
            {
                _jsonOpts = jsonOpts ?? new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
            }

            public async Task ConnectAsync(CancellationToken ct = default)
            {
                if (_running) return;

                try
                {
                    _mmfReq = MemoryMappedFile.CreateOrOpen(REQ_SHM, REQ_SIZE);
                    _mmfResp = MemoryMappedFile.CreateOrOpen(RESP_SHM, RESP_SIZE);
                    _mmfCb = MemoryMappedFile.CreateOrOpen(CB_SHM, CB_SIZE);

                    _viewReq = _mmfReq.CreateViewAccessor();
                    _viewResp = _mmfResp.CreateViewAccessor();
                    _viewCb = _mmfCb.CreateViewAccessor();

                    _semReq = new Semaphore(0, 1, REQ_SEM);
                    _semResp = new Semaphore(0, 1, RESP_SEM);
                    _semCb = new Semaphore(0, int.MaxValue, CB_SEM);

                    _mtxReq = new Semaphore(1, 1, REQ_MTX);
                    _mtxResp = new Semaphore(1, 1, RESP_MTX);
                    _mtxCb = new Semaphore(1, 1, CB_MTX);

                    _running = true;

                    _callbackTask = Task.Run(() => CallbackLoopAsync(_cts.Token), ct);
                    _responseTask = Task.Run(() => ResponseLoopAsync(_cts.Token), ct);
                }
                catch (Exception ex)
                {
                    Dispose();
                    OnTransportError?.Invoke(ex);
                    throw;
                }

                await Task.CompletedTask; // для совместимости с async
            }

            public async Task<TResponse> SendAsync<TRequest, TResponse>(
                TRequest request,
                string command,
                CancellationToken ct = default)
            {
                if (!_running) throw new InvalidOperationException("Transport not running");

                long reqId = Interlocked.Increment(ref _nextRequestId);

                var msg = new Message
                {
                    Id = reqId,
                    Cmd = command,
                    Data = request,
                    t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                string json = JsonSerializer.Serialize(msg, _jsonOpts);
                byte[] payload = Encoding.UTF8.GetBytes(json);

                if (payload.Length > REQ_SIZE - HEADER)
                    throw new ArgumentException($"Request too large ({payload.Length} bytes)");

                var tcs = new TaskCompletionSource<Message>(TaskCreationOptions.RunContinuationsAsynchronously);
                _pending[reqId] = tcs;

                try
                {
                    // Запись запроса
                    _mtxReq!.WaitOne();
                    try
                    {
                        _viewReq!.Write(0, MAGIC);
                        _viewReq.Write(4, VERSION);
                        _viewReq.Write(8, (uint)reqId);
                        _viewReq.Write(12, MSG_TYPE_REQUEST);
                        _viewReq.Write(16, (uint)payload.Length);
                        _viewReq.Write(20, 0u);

                        _viewReq.WriteArray(HEADER, payload, 0, payload.Length);
                    }
                    finally
                    {
                        _mtxReq.Release();
                    }

                    _semReq!.Release();

                    // Ожидание ответа
                    var responseTask = tcs.Task;

                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(45)); // настраиваемый таймаут

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
                        if (!_semResp!.WaitOne(50)) continue;

                        _mtxResp!.WaitOne();
                        try
                        {
                            uint magic = _viewResp!.ReadUInt32(0);
                            if (magic != MAGIC) continue;

                            uint reqId = _viewResp.ReadUInt32(8);
                            uint len = _viewResp.ReadUInt32(16);

                            if (len == 0 || len > RESP_SIZE - HEADER) continue;

                            byte[] buffer = new byte[len];
                            _viewResp.ReadArray(HEADER, buffer, 0, (int)len);

                            string json = Encoding.UTF8.GetString(buffer);
                            var msg = JsonSerializer.Deserialize<Message>(json, _jsonOpts);

                            if (msg != null && _pending.TryRemove((long)msg.Id, out var tcs))
                            {
                                tcs.TrySetResult(msg);
                            }
                        }
                        finally
                        {
                            _mtxResp.Release();
                        }
                    }
                    catch (Exception ex)
                    {
                        OnTransportError?.Invoke(ex);
                        await Task.Delay(300, ct);
                    }
                }
            }

            private async Task CallbackLoopAsync(CancellationToken ct)
            {
                const int pollIntervalMs = 30;

                while (!ct.IsCancellationRequested && _running)
                {
                    try
                    {
                        if (!_semCb!.WaitOne(pollIntervalMs)) continue;

                        _mtxCb!.WaitOne();
                        try
                        {
                            uint magic = _viewCb!.ReadUInt32(0);
                            if (magic != MAGIC) continue;

                            uint len = _viewCb.ReadUInt32(16);
                            if (len == 0 || len > CB_SIZE - HEADER) continue;

                            byte[] buffer = new byte[len];
                            _viewCb.ReadArray(HEADER, buffer, 0, (int)len);

                            string json = Encoding.UTF8.GetString(buffer);
                            var msg = JsonSerializer.Deserialize<Message>(json, _jsonOpts);

                            if (msg != null)
                                DispatchCallback(msg);
                        }
                        finally
                        {
                            _mtxCb.Release();
                        }
                    }
                    catch (Exception ex)
                    {
                        OnTransportError?.Invoke(ex);
                        await Task.Delay(500, ct);
                    }
                }
            }

            private void DispatchCallback(Message msg)
            {
                try
                {
                    switch (msg.Cmd?.ToLowerInvariant())
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
                            OnUnknownCallback?.Invoke(msg.Cmd ?? "unknown");
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
                if (!_running) return;
                _running = false;

                try { _cts.Cancel(); } catch { }

                _callbackTask?.Wait(1200);
                _responseTask?.Wait(1200);

                _viewReq?.Dispose(); _viewResp?.Dispose(); _viewCb?.Dispose();
                _mmfReq?.Dispose(); _mmfResp?.Dispose(); _mmfCb?.Dispose();

                SafeDispose(_semReq); SafeDispose(_semResp); SafeDispose(_semCb);
                SafeDispose(_mtxReq); SafeDispose(_mtxResp); SafeDispose(_mtxCb);

                static void SafeDispose(IDisposable? d)
                {
                    try { d?.Dispose(); } catch { }
                }
            }
        }
    }