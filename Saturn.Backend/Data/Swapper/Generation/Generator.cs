using System.Collections.Generic;
using System.Threading.Tasks;
using Saturn.Backend.Data.SaturnAPI.Models;

namespace Saturn.Backend.Data.Swapper.Generation;

public interface Generator
{
    public Task<List<DisplayItemModel>> Generate();
    public Task<SaturnItemModel> GetItemData(DisplayItemModel item);
}