using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace ImplementUserFacingLogFiles.Logging;

public class JobLoggerFactory(ILogger currentLogger, IHostEnvironment hostEnvironment)
{
    public MonitoringLogger CreateLogger(Guid jobId)
    {
        return new MonitoringLogger(new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.WithProperty("JobId", jobId)
            .WriteTo.File(GetLogFilePath(jobId),
                LogEventLevel.Information,
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message}{NewLine}",
                shared: true)
            .WriteTo.Logger(currentLogger)
            .CreateLogger());
    }

    public async Task<string> GetLogContents(Guid jobId)
    {
        var path = GetLogFilePath(jobId);

        if (!File.Exists(path))
        {
            return string.Empty;
        }

        using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var streamReader = new StreamReader(fileStream))
        {
            return await streamReader.ReadToEndAsync();
        }
    }

    private string GetLogFilePath(Guid jobId)
    {
        return Path.Combine(hostEnvironment.ContentRootPath, "logs", "jobs", $"{jobId:N}.txt");
    }
}