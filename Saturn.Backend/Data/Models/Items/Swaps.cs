using Saturn.Backend.Data.Enums;
using System.Collections.Generic;

namespace Saturn.Backend.Data.Models.Items
{
    public class SaturnItem
    {
        public string ItemDefinition { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Rarity { get; set; }

        public string Icon { get; set; }

        public ItemType Type { get; set; }

        public List<SaturnOption> Options { get; set; }
        public string? Series { get; set; }
        public bool IsConverted { get; set; }
        public string Path { get; set; }
        public Colors PrintColor { get; set; } = Colors.C_WHITE;
        public string? Status { get; set; } = null;
        public MeshDefaultModel SwapModel { get; set; }
        public Dictionary<string, string> Swaps { get; set; }
    }

    public class SaturnOption
    {
        public string Name { get; set; }

        public string Rarity { get; set; }

        public string Icon { get; set; }

        public List<SaturnAsset> Assets { get; set; }
    }

    public class SaturnAsset
    {
        public string ParentAsset { get; set; }

        public List<SaturnSwap> Swaps { get; set; }
    }

    public class SaturnSwap
    {
        public string Search { get; set; }

        public string Replace { get; set; }

        public SwapType Type { get; set; }
    }
}