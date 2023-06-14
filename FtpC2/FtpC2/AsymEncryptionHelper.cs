/*
 * =========================================================================================
 * Author:        Jean-Pierre LESUEUR (@DarkCoderSc)
 * Email:         jplesueur@phrozen.io
 * Website:       https://www.phrozen.io
 * GitHub:        https://github.com/DarkCoderSc
 * Twitter:       https://twitter.com/DarkCoderSc
 * License:       Apache-2.0
 * =========================================================================================
 */

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

/// <summary>
/// This Class allows easily to perform secure encryption using both RSA and AES(256). 
/// </summary>
public class AsymEncryptionHelper : IDisposable
{
    private bool _disposed = false;
    private readonly RSA _RSA;

    protected class EncryptedBundle
    {
        public byte[]? Data { get; set; }
        public byte[]? Key { get; set; }
        public byte[]? IV { get; set; }
    }

    public AsymEncryptionHelper(byte[] publicKey, byte[] privateKey)
    {
        _RSA = RSA.Create();

        _RSA.ImportRSAPublicKey(publicKey, out _);
        _RSA.ImportRSAPrivateKey(privateKey, out _);
    }

    public AsymEncryptionHelper(string encodedPublicKey, string encodedPrivateKey) : this(
        Convert.FromBase64String(encodedPublicKey),
        Convert.FromBase64String(encodedPrivateKey)
    )
    { }

    private byte[] RSAEncrypt(byte[] plainTextData)
    {
        return _RSA.Encrypt(plainTextData, RSAEncryptionPadding.OaepSHA256);
    }

    private byte[] RSADecrypt(byte[] encryptedData)
    {
        return _RSA.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);
    }

    public (byte[] cipherText, byte[] cipherAesKey, byte[] IV) Encrypt(byte[] plainTextData)
    {
        using Aes aes = Aes.Create();

        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;

        byte[] aesKey = aes.Key;
        byte[] cipherAesKey = this.RSAEncrypt(aesKey);

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using (MemoryStream cipherStream = new())
        {
            using (CryptoStream cryptoStream = new(cipherStream, encryptor, CryptoStreamMode.Write))
            {
                using BinaryWriter binaryWriter = new(cryptoStream);

                binaryWriter.Write(plainTextData);
            }

            ///
            return (cipherStream.ToArray(), cipherAesKey, aes.IV);
        }
    }

    public byte[] Decrypt(byte[] cipherText, byte[] cipherAesKey, byte[] IV)
    {
        using Aes aes = Aes.Create();

        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;

        byte[] aesKey = this.RSADecrypt(cipherAesKey);

        aes.Key = aesKey;
        aes.IV = IV;

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream cipherStream = new(cipherText);
        using CryptoStream plainStream = new(cipherStream, decryptor, CryptoStreamMode.Read);
        using MemoryStream stream = new();

        plainStream.CopyTo(stream);

        return stream.ToArray();
    }

    public string EncryptToJson(byte[] plainTextData)
    {
        var (cipherText, cipherAesKey, IV) = Encrypt(plainTextData);

        EncryptedBundle encryptedBundle = new()
        {
            Data = cipherText,
            Key = cipherAesKey,
            IV = IV,
        };

        return JsonSerializer.Serialize(encryptedBundle);
    }

    public string EncryptToJson(string plainTextData)
    {
        return this.EncryptToJson(Encoding.UTF8.GetBytes(plainTextData));
    }

    public byte[] DecryptFromJson(string jsonText)
    {
        EncryptedBundle? encryptedBundle = JsonSerializer.Deserialize<EncryptedBundle>(jsonText);

        if (encryptedBundle == null ||
            encryptedBundle.Data == null ||
            encryptedBundle.Key == null ||
            encryptedBundle.IV == null
            )
            return Array.Empty<byte>();

        return this.Decrypt(encryptedBundle.Data, encryptedBundle.Key, encryptedBundle.IV);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _RSA?.Dispose();

        _disposed = true;
    }
}
