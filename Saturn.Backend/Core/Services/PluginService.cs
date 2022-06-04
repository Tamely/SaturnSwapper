using Newtonsoft.Json;
using Saturn.Backend.Core.Models.SaturnAPI;
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