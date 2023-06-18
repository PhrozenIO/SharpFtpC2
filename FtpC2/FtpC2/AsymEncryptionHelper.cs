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

using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

/// <summary>
/// This Class allows easily to perform secure encryption using both RSA (Asymetric) and AES-GCM 256 (Symetric). 
/// </summary>
public class AsymEncryptionHelper : IDisposable
{
    private bool _disposed = false;
    private readonly RSA _RSA;

    public readonly bool HasPublicKey;
    public readonly bool HasPrivateKey;


    // For debug purpose
    public delegate void AESCallbackDelegate(
        byte[] plainAesKey,
        byte[] cipherAesKey,
        byte[] Nonce,
        byte[] Tag
        );

    public event AESCallbackDelegate? AESCallback;
    //

    protected class EncryptedBundle
    {
        public byte[]? Data { get; set; }
        public byte[]? Key { get; set; }
        public byte[]? Nonce { get; set; }
        public byte[]? Tag { get; set; }
    }

    public static (byte[] publicKey, byte[] privateKey) GenerateRSAKeyPair(int keyLength = 4096)
    {       
        using RSACryptoServiceProvider rsa = new(keyLength);

        byte[] privateKey = rsa.ExportRSAPrivateKey();
        byte[] publicKey = rsa.ExportRSAPublicKey();

        return (publicKey, privateKey);
    }

    public AsymEncryptionHelper(byte[]? publicKey, byte[]? privateKey)
    {
        _RSA = RSA.Create();

        HasPublicKey = publicKey != null;
        HasPrivateKey = privateKey != null;

        if (!HasPublicKey && !HasPrivateKey)
            throw new CryptographicException("You must specify at least a public key or a private key.");

        if (HasPublicKey)
            _RSA.ImportRSAPublicKey(publicKey, out _);

        if (HasPrivateKey)
            _RSA.ImportRSAPrivateKey(privateKey, out _);          
    }

    public AsymEncryptionHelper(string? encodedPublicKey, string? encodedPrivateKey) : this(
        !string.IsNullOrEmpty(encodedPublicKey) ? Convert.FromBase64String(encodedPublicKey) : null,
        !string.IsNullOrEmpty(encodedPrivateKey) ? Convert.FromBase64String(encodedPrivateKey) : null
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

    public (byte[] cipherText, byte[] cipherAesKey, byte[] Nonce, byte[] Tag) Encrypt(byte[] plainText)
    {
        /*using Aes aes = Aes.Create();

        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;

        byte[] aesKey = aes.Key;
        byte[] cipherAesKey = this.RSAEncrypt(aesKey);

        AESCallback?.Invoke(aes.KeySize, aes.Mode, aesKey, cipherAesKey, aes.IV);

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
        }*/

        if (!HasPublicKey)
            throw new CryptographicException("No RSA Public Key Provided.");

        byte[] aesKey = new byte[32]; // * 8 = 256 bits        

        // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator?view=net-7.0?WT_mc_id=SEC-MVP-5005282
        // Generate a random AES key using a cryptographic number generator.
        using RandomNumberGenerator randomGenerator = RandomNumberGenerator.Create();

        // Generate a one-time secure random AES Key.
        randomGenerator.GetBytes(aesKey);

        // Encrypt the AES Key with RSA Public Key.
        byte[] cipherAesKey = this.RSAEncrypt(aesKey);

        // Generate a one-time secure random nonce (usually 12 byte / 96 bits)
        // Typically, generating a random nonce is discouraged due to the risk of nonce collision (which is generally very unlikely)
        // when using the same AES key, as this can compromise security. However, in this scenario, because we are using a one-time
        // random AES key and the probability of a key/nonce collision is negligibly low, this approach doesn't present any significant
        // cryptographic risks.
        byte[] nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        randomGenerator.GetBytes(nonce);

        byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize];
        byte[] cipherText = new byte[plainText.Length];

        // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aesgcm?view=net-7.0?WT_mc_id=SEC-MVP-5005282
        using AesGcm aes = new(aesKey);

        // Encrypt plain-text using our setup, an authentication tag will get returned.
        // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aesgcm.encrypt?view=net-7.0
        aes.Encrypt(nonce, plainText, cipherText, tag);

        AESCallback?.Invoke(aesKey, cipherAesKey, nonce, tag);

        return (cipherText, cipherAesKey, nonce, tag);
    }

    public byte[] Decrypt(byte[] cipherText, byte[] cipherAesKey, byte[] nonce, byte[] authenticatinTag)
    {
        /*
        using Aes aes = Aes.Create();

        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;

        byte[] aesKey = this.RSADecrypt(cipherAesKey);

        aes.Key = aesKey;
        aes.IV = IV;

        AESCallback?.Invoke(aes.KeySize, aes.Mode, aes.Key, cipherAesKey, aes.IV);

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream cipherStream = new(cipherText);
        using CryptoStream plainStream = new(cipherStream, decryptor, CryptoStreamMode.Read);
        using MemoryStream stream = new();

        plainStream.CopyTo(stream);

        return stream.ToArray();
        */

        if (!HasPrivateKey)
            throw new CryptographicException("No RSA Private Key Provided.");

        // Recover the one-time AES Encryption key using our RSA Private key.
        byte[] aesKey = this.RSADecrypt(cipherAesKey);

        byte[] plainText = new byte[cipherText.Length];

        AESCallback?.Invoke(aesKey, cipherAesKey, nonce, authenticatinTag);

        // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aesgcm?view=net-7.0?WT_mc_id=SEC-MVP-5005282
        using AesGcm aes = new(aesKey);        

        // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aesgcm.decrypt?view=net-7.0?WT_mc_id=SEC-MVP-5005282
        aes.Decrypt(nonce, cipherText, authenticatinTag, plainText);

        return plainText;
    }

    public string EncryptToJson(byte[] plainTextData)
    {
        var (cipherText, cipherAesKey, Nonce, Tag) = Encrypt(plainTextData);

        EncryptedBundle encryptedBundle = new()
        {
            Data = cipherText,
            Key = cipherAesKey,
            Nonce = Nonce,
            Tag = Tag,
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
            encryptedBundle.Nonce == null ||
            encryptedBundle.Tag == null
            )
            return Array.Empty<byte>();

        return this.Decrypt(
            encryptedBundle.Data,
            encryptedBundle.Key,
            encryptedBundle.Nonce,
            encryptedBundle.Tag
        );
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
