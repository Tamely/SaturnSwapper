using Radon.CodeAnalysis.Disassembly;

namespace Saturn.Backend.Data.Plugins;

public class PluginModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public string Author { get; set; }
    public string Icon { get; set; }
    public AssemblyInfo AssemblyInfo { get; set; }
}