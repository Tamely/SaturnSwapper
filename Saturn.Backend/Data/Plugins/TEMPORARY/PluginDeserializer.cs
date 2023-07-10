using System;
using System.Text;
using GenericReader;
using Newtonsoft.Json;
using Saturn.Backend.Data.Compression;

namespace Saturn.Backend.Data.Plugins.TEMPORARY;

public class PluginDeserializer
{
    private readonly CompressionBase _oodle = new Oodle();

    private readonly Plugin _deserializedPlugin;

    public PluginDeserializer(byte[] plugin)
    {
        var pluginStream = new GenericBufferReader(plugin);

        PluginStructure structure = new();
        ulong MAGIC = pluginStream.Read<ulong>();
        if (MAGIC != PluginStructure.Magic)
        {
            throw new Exception(
                $"Read plugin magic {MAGIC} does not match with real magic. This most likely means this plugin serializer is outdated.");
        }

        structure.CompressedSize = pluginStream.Read<int>();
        structure.DecompressedSize = pluginStream.Read<int>();

        byte[] compressedData = pluginStream.ReadBytes(structure.CompressedSize);
        structure.PluginData = _oodle.Decompress(compressedData, structure.DecompressedSize);
        structure.PluginData = Convert.FromBase64String(Encoding.UTF8.GetString(structure.PluginData));

        _deserializedPlugin = JsonConvert.DeserializeObject<Plugin>(Encoding.UTF8.GetString(structure.PluginData));
    }

    public Plugin DeserializedPlugin()
    {
        return _deserializedPlugin;
    }
}