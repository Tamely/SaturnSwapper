﻿using System.Threading.Tasks;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.FortniteAPI;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.Utils
{
    public class ItemUtil
    {
        public static async Task UpdateStatus(Cosmetic ParentItem, SaturnItem item, string status, Colors color = Colors.C_WHITE)
        {
            if (item.Status != null)
                ParentItem.Description = status + " | " + item.Status;
            else
                ParentItem.Description = status;
            ParentItem.PrintColor = color;
        }
    }
}