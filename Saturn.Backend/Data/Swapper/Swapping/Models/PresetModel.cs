using System.Collections.Generic;
using Saturn.Backend.Data.SaturnAPI.Models;

namespace Saturn.Backend.Data.Swapper.Swapping.Models;

public class PresetModel
{
    public string PresetName { get; set; }
    public List<Swaps> PresetSwaps { get; set; } = new();
}

public class Swaps
{
    public SaturnItemModel OptionModel { get; set; }
    public SaturnItemModel ItemModel { get; set; }
}