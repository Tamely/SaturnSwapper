using System.Collections.Generic;
using Saturn.Backend.Data.SaturnConfig.Models;

namespace Saturn.Backend.Data.SaturnAPI.Models;

public class OptionsModel
{
    public ConfigModel Config { get; set; }
    public List<string> ownedCosmetics { get; set; }
}