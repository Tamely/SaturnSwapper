using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Saturn.Backend.Data.Utils.ReadPlugins;

public class DotSaturn
{
    public static string Read(string filePath)
    {
        var data = File.ReadAllBytes(filePath);

        var hash = new byte[] { };
        int index;
        for (index = 0; index <= 16; index++)
        {
            AddByteToArray(ref hash, new byte[] { data[index] });
        }

        var compLength = (int)data[index];
        index++;
        var decompLength = (int)data[index];
        Logger.Log(decompLength.ToString());
        index ++;

        var pluginData = new byte[compLength];

        Buffer.BlockCopy(data, index, pluginData, 0, compLength);

        byte[] decompPluginData = new byte[decompLength];

        Oodle.Decompress(pluginData, ref decompPluginData);

        using (var md5 = MD5.Create())
        {
            var newHash = md5.ComputeHash(decompPluginData);
            
            //if (hash != newHash)
                //throw new Exception("Hash mismatch");
        }
        
        
        // get string from byte array
        var pluginString = Encoding.UTF8.GetString(decompPluginData);

        return pluginString;
    }
    
    
    public static void Write(string filePath, string json)
    {
        byte[] data = new byte[] { };
        
        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(json));

            AddByteToArray(ref data, hash);
        }

        AddByteToArray(ref data, new byte[]{0});

        var pluginData =
            Crypto.EncryptString(json, "0x2CCDFD22AD74FBFEE693A81AC11ACE57E6D10D0B8AC5FA90E793A130BC540ED4");

        var decompLength = pluginData.Length;

        pluginData = Oodle.Compress(pluginData);
        
        var compLength = pluginData.Length;
        
        AddByteToArray(ref data, new byte[]{(byte)compLength});
        AddByteToArray(ref data, new byte[]{(byte)decompLength});
        AddByteToArray(ref data, new byte[]{0});
        AddByteToArray(ref data, pluginData);

        File.WriteAllBytes(filePath, data);
    }
    
    public static byte[] AddByteToArray(ref byte[] bArray, byte[] newByte)
    {
        var newArray = new byte[bArray.Length + newByte.Length];
        Buffer.BlockCopy(bArray, 0, newArray, 0, bArray.Length);
        Buffer.BlockCopy(newByte, 0, newArray, bArray.Length, newByte.Length);

        bArray = newArray;
        return newArray;
    }

    
}