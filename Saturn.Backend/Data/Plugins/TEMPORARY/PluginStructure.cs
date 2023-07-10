namespace Saturn.Backend.Data.Plugins.TEMPORARY;

public class PluginStructure
{
    public const ulong Magic = 0x32132713612;
    public int CompressedSize { get; set; }
    public int DecompressedSize { get; set; }
    public byte[] PluginData { get; set; }
}