using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace ImplementUserFacingLogFiles.Logging;

public class MonitoringLogger(ILogger internalLogger) : ILogger
{
    public bool HasErrors { get; private set; }

    public bool HasWarnings { get; private set; }

    public void Write(LogEvent logEvent)
    {
        switch (logEvent.Level)
        {
            case LogEventLevel.Error:
                HasErrors = true;
                break;
            case LogEventLevel.Warning:
                HasWarnings = true;
                break;
        }

        internalLogger.Write(logEvent);
    }
}