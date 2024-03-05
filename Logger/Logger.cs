using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LoggingLibrary
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public static class Logger
    {
        public static string LogFilePath = "logfile.txt";
        private static readonly ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private static readonly ManualResetEventSlim waitEvent = new ManualResetEventSlim(false);
        private static bool isWriting = false;

        static Logger()
        {
            Task.Run(() => WriteLogsToFile());
        }

        public static void Log(LogLevel level, string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = GetConsoleColor(level);
            Console.WriteLine($"[{DateTime.Now}] [{level.ToString().ToUpper()}] {message}");
            Console.ForegroundColor = originalColor;

            string logEntry = $"[{DateTime.Now}] [{level.ToString().ToUpper()}] {message}";
            logQueue.Enqueue(logEntry);

            waitEvent.Set();
        }

        private static async Task WriteLogsToFile()
        {
            while (true)
            {
                waitEvent.Wait();
                await semaphoreSlim.WaitAsync();

                try
                {
                    if (!logQueue.IsEmpty && !isWriting)
                    {
                        isWriting = true;
                        using (StreamWriter writer = File.AppendText(LogFilePath))
                        {
                            while (logQueue.TryDequeue(out string logEntry))
                            {
                                await writer.WriteLineAsync(logEntry);
                            }
                        }
                        isWriting = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write logs to file: {ex.Message}");
                }
                finally
                {
                    semaphoreSlim.Release();
                    waitEvent.Reset();
                }
            }
        }

        private static ConsoleColor GetConsoleColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                    return ConsoleColor.Green;
                case LogLevel.Warning:
                    return ConsoleColor.Yellow;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                default:
                    return ConsoleColor.White;
            }
        }
    }
}