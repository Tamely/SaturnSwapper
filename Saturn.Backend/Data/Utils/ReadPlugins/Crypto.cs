using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Saturn.Backend.Data.Utils.ReadPlugins;

public class Crypto
{
    private const int AesBlockByteSize = 128 / 8;

    private const int PasswordSaltByteSize = 128 / 8;
    private const int PasswordByteSize = 256 / 8;
    private const int PasswordIterationCount = 100_000;

    private const int SignatureByteSize = 256 / 8;

    private const int MinimumEncryptedMessageByteSize =
        PasswordSaltByteSize + // auth salt
        PasswordSaltByteSize + // key salt
        AesBlockByteSize + // IV
        AesBlockByteSize + // cipher text min length
        SignatureByteSize; // signature tag

    private static readonly Encoding StringEncoding = Encoding.UTF8;
    private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

    public static byte[] EncryptString(string toEncrypt, string password)
    {
        // encrypt
        var keySalt = GenerateRandomBytes(PasswordSaltByteSize);
        var key = GetKey(password, keySalt);
        var iv = GenerateRandomBytes(AesBlockByteSize);

        byte[] cipherText;
        using (var aes = CreateAes())
        using (var encryptor = aes.CreateEncryptor(key, iv))
        {
            var plainText = StringEncoding.GetBytes(toEncrypt);
            cipherText = encryptor
                .TransformFinalBlock(plainText, 0, plainText.Length);
        }

        // sign
        var authKeySalt = GenerateRandomBytes(PasswordSaltByteSize);
        var authKey = GetKey(password, authKeySalt);

        var result = MergeArrays(
            additionalCapacity: SignatureByteSize,
            authKeySalt, keySalt, iv, cipherText);

        using (var hmac = new HMACSHA256(authKey))
        {
            var payloadToSignLength = result.Length - SignatureByteSize;
            var signatureTag = hmac.ComputeHash(result, 0, payloadToSignLength);
            signatureTag.CopyTo(result, payloadToSignLength);
        }

        return result;
    }

    public static string DecryptToString(byte[] encryptedData, string password)
    {
        if (encryptedData is null
            || encryptedData.Length < MinimumEncryptedMessageByteSize)
        {
            throw new ArgumentException("Invalid length of encrypted data");
        }

        var authKeySalt = encryptedData
            .AsSpan(0, PasswordSaltByteSize).ToArray();
        var keySalt = encryptedData
            .AsSpan(PasswordSaltByteSize, PasswordSaltByteSize).ToArray();
        var iv = encryptedData
            .AsSpan(2 * PasswordSaltByteSize, AesBlockByteSize).ToArray();
        var signatureTag = encryptedData
            .AsSpan(encryptedData.Length - SignatureByteSize, SignatureByteSize).ToArray();

        var cipherTextIndex = authKeySalt.Length + keySalt.Length + iv.Length;
        var cipherTextLength =
            encryptedData.Length - cipherTextIndex - signatureTag.Length;

        var authKey = GetKey(password, authKeySalt);
        var key = GetKey(password, keySalt);

        // verify signature
        using (var hmac = new HMACSHA256(authKey))
        {
            var payloadToSignLength = encryptedData.Length - SignatureByteSize;
            var signatureTagExpected = hmac
                .ComputeHash(encryptedData, 0, payloadToSignLength);

            // constant time checking to prevent timing attacks
            var signatureVerificationResult = 0;
            for (int i = 0; i < signatureTag.Length; i++)
            {
                signatureVerificationResult |= signatureTag[i] ^ signatureTagExpected[i];
            }

            if (signatureVerificationResult != 0)
            {
                throw new CryptographicException("Invalid signature");
            }
        }

        // decrypt
        using (var aes = CreateAes())
        {
            using (var encryptor = aes.CreateDecryptor(key, iv))
            {
                var decryptedBytes = encryptor
                    .TransformFinalBlock(encryptedData, cipherTextIndex, cipherTextLength);
                return StringEncoding.GetString(decryptedBytes);
            }
        }
    }

    private static Aes CreateAes()
    {
        var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }

    private static byte[] GetKey(string password, byte[] passwordSalt)
    {
        var keyBytes = StringEncoding.GetBytes(password);

        using (var derivator = new Rfc2898DeriveBytes(
            keyBytes, passwordSalt, 
            PasswordIterationCount, HashAlgorithmName.SHA256))
        {
            return derivator.GetBytes(PasswordByteSize);
        }
    }

    private static byte[] GenerateRandomBytes(int numberOfBytes)
    {
        var randomBytes = new byte[numberOfBytes];
        Random.GetBytes(randomBytes);
        return randomBytes;
    }

    private static byte[] MergeArrays(int additionalCapacity = 0, params byte[][] arrays)
    {
        var merged = new byte[arrays.Sum(a => a.Length) + additionalCapacity];
        var mergeIndex = 0;
        for (int i = 0; i < arrays.GetLength(0); i++)
        {
            arrays[i].CopyTo(merged, mergeIndex);
            mergeIndex += arrays[i].Length;
        }

        return merged;
    }
}