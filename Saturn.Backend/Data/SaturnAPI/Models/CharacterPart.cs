using System.Collections.Generic;

namespace Saturn.Backend.Data.SaturnAPI.Models;

public class CharacterPart
{
    public string Path { get; set; }
    public Dictionary<string, string> Enums { get; set; }
}