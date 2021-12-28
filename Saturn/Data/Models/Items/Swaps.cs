using Saturn.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Data.Models.Items
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
