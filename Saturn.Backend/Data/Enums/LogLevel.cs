using System.ComponentModel;

namespace Saturn.Backend.Data.Enums
{
    public enum LogLevel
    {
        [Description("DBG")] Debug,

        [Description("INF")] Info,

        [Description("WRN")] Warning,

        [Description("ERR")] Error,

        [Description("FTL")] Fatal
    }
}