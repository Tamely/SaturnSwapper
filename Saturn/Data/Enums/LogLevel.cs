using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Data.Enums
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
