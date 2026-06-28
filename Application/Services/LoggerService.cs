using NAVMetadata.Abstractions;
using Serilog;

namespace NAVMetadata.Services;

public sealed class LoggerService : ILoggerService
{
    public void LogInfo(string message) => Log.Information(message);
    public void LogWarning(string message) => Log.Warning(message);
    public void LogError(string message, Exception? exception = null)
    {
        if (exception is null)
            Log.Error(message);
        else
            Log.Error(exception, message);
    }
}
