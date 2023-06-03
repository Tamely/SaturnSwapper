using System;

namespace Saturn.Backend.Data.Services.OobeServiceUtils;

[Flags]
public enum OobeType
{
    NotStarted               = 0x00000000,
    
    // Picking cosmetics
    ExplainPickCosmetics     = 0x00000001,
    ClickEndPickCosmetics    = 0x00000002,
    
    // Dashboard
    ExplainDashboard         = 0x00000004,
    ExplainWarning           = 0x00000008,
    ClickEndDashboard        = 0x00000010,

    // Cosmetics
    ExplainCosmetics         = 0x00000020,
    ClickCosmetic            = 0x00000040,
    ClickOption              = 0x00000080,
    ClickEndCosmetics        = 0x00000100,
    
    // Settings
    ExplainSettings          = 0x00000200,
    ShowSwappedItems         = 0x00000400,
    LaunchFortnite           = 0x00000800,
    
    // Skip
    Done                     = 0x40000000
}