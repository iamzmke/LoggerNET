<a href="#Logger.NET"><img src="http://readme-typing-svg.herokuapp.com?font=VT323&size=90&duration=2000&pause=1000&color=F70000&center=true&random=false&width=1100&height=140&lines=%E2%98%A6+Logger.NET+%E2%98%A6;%E2%98%A6+By+Smoke+%E2%98%A6" alt="Ω" /></a>

# Logger.NET

Logger.NET is a lightweight and efficient logging library for C# applications, offering customizable logging capabilities for both console and file output.

## Features

- **Console Logging:** Easily log messages to the console with customizable colors based on different log levels (info, warning, error), improving readability and aiding in identifying critical log messages.
- **File Logging:** Asynchronously log messages to a file, enabling developers to persist log data for debugging and troubleshooting. Logger.NET efficiently handles concurrent log writes and optimizes file I/O operations for improved performance.
- **Efficiency:** Utilizes asynchronous programming techniques and concurrent data structures to ensure efficient logging operations without blocking the main thread. This ensures minimal performance overhead and maintains application responsiveness.
- **Simplicity:** Designed with simplicity in mind, Logger.NET provides a straightforward API for integrating logging functionality into C# applications with ease, catering to developers of all skill levels.

## Usage

```csharp
// Configure Logger.NET
Logger.Configure(config =>
{
    config.LogFilePath = "log.txt";
    config.MinimumLogLevel = LogLevel.Info;
    config.UseWatermark = true;
});

// Log messages
Logger.Log(LogLevel.Info, "Informational message");
Logger.Log(LogLevel.Warning, "Warning message");
Logger.Log(LogLevel.Error, "Error message");
