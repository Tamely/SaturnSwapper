using System;
using Saturn.Backend.Data.SaturnAPI.Models.UnrealModels;

namespace Saturn.Backend.Data.Swapper.Swapping;

public class ByteFinder
{
    public static int FindExpressionGUID(byte[] data, string expressionGUID)
    {
        byte[] expressionGUIDBytes = ConvertGUIDToBytes(expressionGUID);
        return 0;  //IoStoreWriter.IndexOfSequence(data, expressionGUIDBytes);
    }

    private static byte[] ConvertGUIDToBytes(string expressionGUID)
    {
        byte[] output = new byte[16];
        string[] parts = expressionGUID.Split('|');
        
        for (int i = 0; i < parts.Length; i++)
        {
            Logger.Log("Parsing int " + parts[i]);
            byte[] partBytes = BitConverter.GetBytes(Convert.ToUInt32(parts[i], 10));
            Array.Copy(partBytes, 0, output, i * 4, 4);
        }

        return output;
    }

    public static byte[] ConvertVectorToBytes(FVector vector)
    {
        byte[] output = new byte[12];
        byte[] xBytes = BitConverter.GetBytes(vector.X);
        byte[] yBytes = BitConverter.GetBytes(vector.Y);
        byte[] zBytes = BitConverter.GetBytes(vector.Z);

        Array.Copy(xBytes, 0, output, 0, 4);
        Array.Copy(yBytes, 0, output, 4, 4);
        Array.Copy(zBytes, 0, output, 8, 4);

        return output;
    }

    public static byte[] ConvertFLinearColorToBytes(FLinearColor color)
    {
        byte[] output = new byte[16];
        byte[] r = BitConverter.GetBytes(color.R);
        byte[] g = BitConverter.GetBytes(color.G);
        byte[] b = BitConverter.GetBytes(color.B);
        byte[] a = BitConverter.GetBytes(color.A);

        Array.Copy(r, 0, output, 0, 4);
        Array.Copy(g, 0, output, 4, 4);
        Array.Copy(b, 0, output, 8, 4);
        Array.Copy(a, 0, output, 12, 4);

        return output;
    }
}