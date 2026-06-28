namespace NAVMetadata.Abstractions;

/// <summary>Application logging facade (backed by Serilog).</summary>
public interface ILoggerService
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? exception = null);
}
