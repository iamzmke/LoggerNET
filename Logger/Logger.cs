using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public enum LogLevel
{
    Trace,
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

public class LoggerConfiguration
{
    public string LogFilePath { get; set; } = "logfile.txt";
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Info;
    public ConsoleColor WatermarkColor { get; set; } = ConsoleColor.Gray;
    public string WatermarkText { get; set; } = "LOG";
    public bool UseWatermark { get; set; } = false;
    public ConsoleColor MessageColor { get; set; } = ConsoleColor.White;
}

public static class Logger
{
    private static readonly ConcurrentQueue<string> LogQueue = new ConcurrentQueue<string>();
    private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
    private static readonly ManualResetEventSlim WaitEvent = new ManualResetEventSlim(false);
    private static bool IsWritingToFile = false;
    private static LoggerConfiguration _configuration = new LoggerConfiguration();

    public static LoggerConfiguration Configuration
    {
        get => _configuration;
        set
        {
            _configuration = value;
            if (_configuration.UseWatermark)
                SetWatermark();
        }
    }

    static Logger()
    {
        Task.Run(() => WriteLogsToFile());
        if (_configuration.UseWatermark)
            SetWatermark();
    }

    private static void SetWatermark()
    {
        Console.ForegroundColor = _configuration.WatermarkColor;
        Console.WriteLine($"[{DateTime.Now}] {_configuration.WatermarkText}");
        Console.ResetColor();
    }

    public static void Log(LogLevel level, string message)
    {
        if (level < _configuration.MinimumLogLevel)
            return;

        ConsoleColor originalColor = Console.ForegroundColor;

        if (_configuration.UseWatermark)
            Console.WriteLine($"[{DateTime.Now}] {_configuration.WatermarkText}");

        Console.ForegroundColor = ConsoleColor.DarkGray; // For timestamp
        Console.Write($"[{DateTime.Now}] ");
        Console.ForegroundColor = GetConsoleColor(level);
        Console.Write("[");
        Console.ForegroundColor = _configuration.MessageColor;
        Console.Write($"{level}");
        Console.ForegroundColor = GetConsoleColor(level);
        Console.Write("] ");
        Console.ForegroundColor = _configuration.MessageColor;
        Console.WriteLine($"{message}");
        Console.ForegroundColor = originalColor;

        string logEntry = _configuration.UseWatermark ? $"[{DateTime.Now}] {_configuration.WatermarkText} - " : "";
        logEntry += $"[{DateTime.Now}] [{level}] {message}";
        LogQueue.Enqueue(logEntry);

        WaitEvent.Set();
    }

    private static async Task WriteLogsToFile()
    {
        while (true)
        {
            WaitEvent.Wait();
            await SemaphoreSlim.WaitAsync();

            try
            {
                if (!LogQueue.IsEmpty && !IsWritingToFile)
                {
                    IsWritingToFile = true;
                    using (StreamWriter writer = File.AppendText(_configuration.LogFilePath))
                    {
                        while (LogQueue.TryDequeue(out string logEntry))
                        {
                            await writer.WriteLineAsync(logEntry);
                        }
                    }
                    IsWritingToFile = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write logs to file: {ex.Message}");
            }
            finally
            {
                SemaphoreSlim.Release();
                WaitEvent.Reset();
            }
        }
    }

    private static ConsoleColor GetConsoleColor(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Trace:
                return ConsoleColor.Gray;
            case LogLevel.Debug:
                return ConsoleColor.DarkGray;
            case LogLevel.Info:
                return ConsoleColor.Green;
            case LogLevel.Warning:
                return ConsoleColor.Yellow;
            case LogLevel.Error:
                return ConsoleColor.Red;
            case LogLevel.Fatal:
                return ConsoleColor.DarkRed;
            default:
                return ConsoleColor.White;
        }
    }

    public static void LogException(Exception ex)
    {
        Log(LogLevel.Error, $"Exception occurred: {ex.GetType().Name} - Message: {ex.Message} - StackTrace: {ex.StackTrace}");
    }

    public static void LogInfo(string message, params object[] args)
    {
        Log(LogLevel.Info, string.Format(message, args));
    }

    public static void LogWarning(string message, params object[] args)
    {
        Log(LogLevel.Warning, string.Format(message, args));
    }

    public static void LogError(string message, params object[] args)
    {
        Log(LogLevel.Error, string.Format(message, args));
    }

    public static void LogFatal(string message, params object[] args)
    {
        Log(LogLevel.Fatal, string.Format(message, args));
    }

    public static void LogTrace(string message, params object[] args)
    {
        Log(LogLevel.Trace, string.Format(message, args));
    }

    public static void LogDebug(string message, params object[] args)
    {
        Log(LogLevel.Debug, string.Format(message, args));
    }

    public static void LogToFile(string filePath, string message)
    {
        try
        {
            using (StreamWriter writer = File.AppendText(filePath))
            {
                writer.WriteLine($"[{DateTime.Now}] {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write log to file: {ex.Message}");
        }
    }

    public static void Configure(Action<LoggerConfiguration> configure)
    {
        configure(Configuration);
    }
}
