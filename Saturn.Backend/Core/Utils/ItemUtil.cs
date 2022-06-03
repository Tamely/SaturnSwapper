using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.FortniteAPI;
using Saturn.Backend.Core.Models.Items;
using System.Threading.Tasks;

namespace Saturn.Backend.Core.Utils
{
    public class ItemUtil
    {
        public static async Task UpdateStatus(Cosmetic ParentItem, SaturnItem? item = null, string status = "", Colors color = Colors.C_WHITE)
        {
            if (item?.Status != null)
                ParentItem.Description = status + " | " + item.Status;
            else
                ParentItem.Description = status;
            ParentItem.PrintColor = color;
        }
    }
}