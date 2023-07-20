using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Saturn.Backend.Data.Compression;

namespace Saturn.Backend.Data.Plugins.TEMPORARY;

public class PluginSerializer
{
    private readonly CompressionBase _oodle = new Oodle();
    private readonly List<byte> _pluginData = new();

    public PluginSerializer(Plugin plugin)
    {
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(plugin));
        byte[] decompressedData = Encoding.UTF8.GetBytes(Convert.ToBase64String(plainTextBytes));
        byte[] compressedData = _oodle.Compress(decompressedData);

        _pluginData.AddRange(BitConverter.GetBytes(PluginStructure.Magic));
        _pluginData.AddRange(BitConverter.GetBytes(compressedData.Length));
        _pluginData.AddRange(BitConverter.GetBytes(decompressedData.Length));
        
        _pluginData.AddRange(compressedData);
    }

    public byte[] Data()
    {
        return _pluginData.ToArray();
    }
}