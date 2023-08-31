using System.ComponentModel;

namespace Radon.Common;

public enum LogLevel
{
    [Description("TRC")] Trace,
    [Description("INF")] Info,
    [Description("WRN")] Warning,
    [Description("ERR")] Error
}