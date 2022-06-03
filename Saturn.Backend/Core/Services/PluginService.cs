using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using Saturn.Backend.Core.Models.SaturnAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saturn.Backend.Core.Services;

public interface IPluginService
{
    public Task<PluginModel> LoadPlugin(string saturnPlugin);
}

public class PluginService : IPluginService
{
    public async Task<PluginModel> LoadPlugin(string saturnPlugin)
    {
        return JsonConvert.DeserializeObject<PluginModel>(saturnPlugin) ?? new PluginModel();
    }
}