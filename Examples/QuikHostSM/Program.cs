using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;  // NuGet: Newtonsoft.Json

namespace QuikSharpSHMHost
{
    class Program
    {
        private const string SHM_NAME = "QuikSharp_SHM_v2";
        private const string SEM_CS2LUA = "QuikSharp_CS2Lua";   // C# → Lua (сигнал о запросе)
        private const string SEM_LUA2CS = "QuikSharp_Lua2CS";   // Lua → C# (сигнал об ответе)
        private static Semaphore? semLua2CsSync;
        private static Semaphore? semLua2CsCallback;
        private const int SHM_SIZE = 4 * 1024 * 1024;     // 4 МБ
        private const int HEADER_SIZE = 24;

        // Структура заголовка (точно как в Lua)
        // 0-3   magic     uint32   = 0x5155494B ("QUIK")
        // 4-7   version   uint32   = 2
        // 8-11  req_id    uint32
        //12-15  msg_type  uint32   (1=request, 2=response)
        //16-19  body_len  uint32
        //20-23  reserved  uint32

        private static MemoryMappedFile? mmf;
        private static MemoryMappedViewAccessor? accessor;
        private static Semaphore? semCs2Lua;
        private static Semaphore? semLua2Cs;

        private static async Task Main(string[] args)
        {
            Console.WriteLine("QuikSharp SHM Host (C#) — тестовый клиент");
            try
            {
                InitializeIPC();
                await Task.Delay(2500); // QUIK обычно требует 1.5–3 сек на полную инициализацию

                var cts = new CancellationTokenSource();
                _ = Task.Run(() => ListenCallbacksAsync(cts.Token)); // ← запуск потока колбэков!

                await SendTestRequestAsync("getClassesList", 1001);
                await SendTestRequestAsync("ping", 1002, "Ping"); // строка, а не объект
                await SendTestRequestAsync("isConnected", 1003);
                await SendTestRequestAsync("getWorkingFolder", 1004);
                await SendTestRequestAsync("getInfoParam", 1005, "SERVERTIME");
                await SendTestRequestAsync("getInfoParam", 1006, "IS_SERVER_CONNECTED");
                await SendTestRequestAsync("getSecurityInfo", 1008, "TQBR|SBER");

                Console.WriteLine("\nНажмите Enter для выхода...");
                Console.ReadLine();
                cts.Cancel();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Критическая ошибка: " + ex);
            }
            finally
            {
                Cleanup();
            }
        }
        private static void InitializeIPC()
        {
            mmf = MemoryMappedFile.CreateOrOpen(SHM_NAME, SHM_SIZE, MemoryMappedFileAccess.ReadWrite);
            accessor = mmf.CreateViewAccessor();

            semCs2Lua = Semaphore.OpenExisting("QuikSharp_CS2Lua");
            semLua2CsSync = Semaphore.OpenExisting("QuikSharp_Lua2CS_Sync");
            semLua2CsCallback = Semaphore.OpenExisting("QuikSharp_Lua2CS_Callback");

            Console.WriteLine("IPC открыт (два семафора для sync и callback)");
        }


        private static Random rnd = new Random();

        private static async Task SendTestRequestAsync(string cmd, int reqId, object? payload = null)
        {
            int nonce = rnd.Next(1000000, 9999999); // уникальный маркер

            var request = new
            {
                cmd,
                req_id = reqId,
                nonce,  // ← добавляем
                data = payload
            };

            string json = JsonConvert.SerializeObject(request);
            byte[] data = Encoding.UTF8.GetBytes(json);
            int len = data.Length;

            Console.WriteLine($"→ {cmd} (req_id={reqId})");
            const int MAX_SAFE_BODY = 2 * 8192; // 1 МБ — достаточно для тестов
            byte[] zero = new byte[MAX_SAFE_BODY];
            accessor.WriteArray(HEADER_SIZE, zero, 0, MAX_SAFE_BODY);
            try
            {
                accessor.WriteArray(HEADER_SIZE, data, 0, len);
                accessor.Write(0, 0x5155494B);
                accessor.Write(4, 2);
                accessor.Write(8, reqId);
                accessor.Write(12, 1);
                accessor.Write(16, len);
                accessor.Write(20, 0);

                semCs2Lua.Release();
                await Task.Delay(50);
                bool got = semLua2CsSync.WaitOne(12000);
                if (!got)
                {
                    Console.WriteLine("Timeout (sync response)");
                    return;
                }

                int respReqId = accessor.ReadInt32(8);
                int bodyLen = accessor.ReadInt32(16);

                if (respReqId != reqId)
                {
                    Console.WriteLine($"Несовпадение req_id: ожидался {reqId}, пришёл {respReqId}");
                    semLua2CsSync.Release();
                    return;
                }

                if (bodyLen == 0)
                {
                    Console.WriteLine("Получен пустой ответ (возможно heartbeat)");
                    semLua2CsSync.Release();
                    return;
                }

                byte[] resp = new byte[bodyLen];
                accessor.ReadArray(HEADER_SIZE, resp, 0, bodyLen);
                //Console.WriteLine($"Сырые байты ответа (первые 100):");
                //for (int i = 0; i < Math.Min(100, resp.Length); i++)
                //{
                //    Console.Write($"{resp[i]:X2} ");
                //}
                //Console.WriteLine();

                //// и полная длина
                //Console.WriteLine($"Длина прочитанного тела: {resp.Length} байт (ожидалось {bodyLen})");
                string respJson = Encoding.UTF8.GetString(resp);

                Console.WriteLine($"← {respJson}");

                try
                {
                    dynamic obj = JsonConvert.DeserializeObject(respJson);
                    if (obj.nonce != nonce)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"!!! NONCE НЕ СОВПАДАЕТ: ожидался {nonce}, пришёл {obj.nonce}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine("Nonce совпал — ответ точно на этот запрос");
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе {cmd}: {ex.Message}");
            }
            finally
            {
                semLua2CsSync?.Release(); // всегда освобождаем, даже при ошибке
            }
        }

        private static void Cleanup()
        {
            try
            {
                semCs2Lua?.Close();
                semLua2CsSync?.Close();
                semLua2CsCallback?.Close();
                accessor?.Dispose();
                mmf?.Dispose();
                Console.WriteLine("Ресурсы освобождены");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при очистке: " + ex.Message);
            }
        }

        private static async Task ListenCallbacksAsync(CancellationToken ct)
        {
            Console.WriteLine("Поток колбэков запущен (отдельный семафор)");
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    bool got = semLua2CsCallback.WaitOne(1500);
                    if (!got) continue;

                    int bodyLen = accessor.ReadInt32(16);
                    if (bodyLen == 0) continue;

                    byte[] bytes = new byte[bodyLen];
                    accessor.ReadArray(HEADER_SIZE, bytes, 0, bodyLen);
                    string json = Encoding.UTF8.GetString(bytes);

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"[CALLBACK] {json}");
                    Console.ResetColor();

                    // Можно здесь добавить десериализацию и вызов обработчиков
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка в callback-потоке: " + ex.Message);
                    await Task.Delay(1000, ct);
                }
            }
        }

    }
}