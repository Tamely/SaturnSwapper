using Saturn.Data.Enums;
using Saturn.Data.Models.FortniteAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Data.Utils
{
    public class ItemUtil
    {
        public static async Task UpdateStatus(Cosmetic item, string status, Colors color = Colors.C_WHITE)
        {
            item.Description = status;
            item.PrintColor = color;
        }
    }
}
