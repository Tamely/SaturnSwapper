using System;
using System.IO;
using System.Security.Cryptography;

namespace Saturn.Backend.Data.Manifest;

public class SHA1Hash
{
    public static string HashByteArray(byte[] input) => Convert.ToHexString(SHA1.Create().ComputeHash(input));
    public static string HashFile(string path) => File.Exists(path) ? Convert.ToHexString(SHA1.Create().ComputeHash(File.ReadAllBytes(path))) : string.Empty;
    public static string HashFileStream(FileStream fs) => File.Exists(fs.Name) ? Convert.ToHexString(SHA1.Create().ComputeHash(fs)) : string.Empty;
    public static string HashStream(Stream fs) => Convert.ToHexString(SHA1.Create().ComputeHash(fs));
}