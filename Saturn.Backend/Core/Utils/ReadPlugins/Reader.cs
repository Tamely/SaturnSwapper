using GenericReader;
using Newtonsoft.Json;
using Oodle.NET;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.SaturnAPI;
using System.IO;
using System.Text;

namespace Saturn.Backend.Core.Utils.ReadPlugins;

public class Reader
{
    public Reader(string filePath, out PluginModel plugin) : this(new GenericStreamReader(filePath), out plugin) {}
    public Reader (Stream fileStream, out PluginModel plugin) : this(new GenericStreamReader(fileStream), out plugin) {}
    public Reader (byte[] fileBuffer, out PluginModel plugin) : this(new GenericBufferReader(fileBuffer), out plugin) {}

    internal Reader(IGenericReader fileReader, out PluginModel plugin)
    {
        var securityCheck = fileReader.Read<ulong>();

        var compSize = fileReader.Read<uint>();
        var decompSize = fileReader.Read<uint>();

        if (securityCheck != DotSaturn.GenerateSecurityCheck(compSize, decompSize))
        {
            Logger.Log("Security check failed. Plugin was altered.");
            throw new FileLoadException("Security check failed. Plugin was altered.");
        }

        byte[] compData = fileReader.ReadBytes((int)compSize);
        var decompData = new byte[decompSize];

        using var decompressor = new OodleCompressor(Config.OodlePath);

        unsafe
        {
            var result = decompressor.DecompressBuffer(compData, compSize, decompData, decompSize, OodleLZ_FuzzSafe.No,
                OodleLZ_CheckCRC.No, OodleLZ_Verbosity.None, 0L, 0L, 0L, 0L, 0L, 0L,
                OodleLZ_Decode_ThreadPhase.Unthreaded);

            if (result != decompSize)
            {
                Logger.Log("Decompression of file failed. Expected size: " + decompSize + " Actual size: " + result, LogLevel.Fatal);
                throw new FileLoadException("Decompression of file failed.");
            }
        }

        plugin = JsonConvert.DeserializeObject<PluginModel>(Encoding.ASCII.GetString(decompData));
    }
}