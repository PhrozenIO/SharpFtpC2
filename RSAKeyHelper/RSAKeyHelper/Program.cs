/*
 * =========================================================================================
 * Project:       SharpFtpC2 - RSA Key Helper (v1.0)
 * 
 * Description:   SharpFtpShell is a compact C# project that showcases the technique
 *                of tunneling Command and Control (C2) communication via FTP(S).
 * 
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

class Program
{
    public static void WriteTitle(string title)
    {
        string line = new('=', title.Length + 4);

        Console.WriteLine(line);

        Console.Write("= ");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(title);
        Console.ResetColor();

        Console.WriteLine(" =");

        Console.WriteLine(line);
        Console.WriteLine();
    }

    public static void WriteLabel(string label)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"@{label}");
        Console.ResetColor();
    }

    public static void DisplayKeyValue(string key, string value, ConsoleColor color = ConsoleColor.Green)
    {
        Console.ForegroundColor = color;
        Console.Write("* ");
        Console.ResetColor();
        Console.Write($"{key} : ");
        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ResetColor();

        Console.WriteLine();
    }

    public static string FormatByteArrayAsString(byte[] bytes, int? maxLength = null)
    {
        var candidates = maxLength.HasValue ? bytes.Take(maxLength.Value) : bytes;

        string formatedByteString = string.Join(":", candidates.Select(b => b.ToString("x2")));

        if (maxLength.HasValue)
            formatedByteString += "...more...";

        return formatedByteString;
    }

    public static string ComputeFingerprint(byte[] data)
    {
        using SHA1 sha1 = SHA1.Create();

        byte[] hash = sha1.ComputeHash(data);

        return FormatByteArrayAsString(hash); 
    }    

    public static void Main(string[] args)
    {
        var (publicKey, privateKey) = AsymEncryptionHelper.GenerateRSAKeyPair(4096);

        WriteTitle("Generated RSA Key Pair");

        // Output Public and Private Keys as Base64
        WriteLabel("PubKey:");
        Console.WriteLine(Convert.ToBase64String(publicKey));
        WriteLabel("EOF");

        Console.WriteLine();        

        WriteLabel("PrivKey:");
        Console.WriteLine(Convert.ToBase64String(privateKey));
        WriteLabel("EOF");

        // Output Public and Private Keys Fingerprint
        Console.WriteLine();
        DisplayKeyValue("Public Key Fingerprint", ComputeFingerprint(publicKey));
        DisplayKeyValue("Public Key Guid Fingerprint", SharedUtilities.ComputeFingerprintAsGuid(publicKey).ToString()); 
        
        DisplayKeyValue("Public Key Fingerprint", ComputeFingerprint(privateKey));
        DisplayKeyValue("Private Key Guid Fingerprint", SharedUtilities.ComputeFingerprintAsGuid(privateKey).ToString());
        Console.WriteLine();

        // Test encryption with generated keys
        Console.WriteLine("Type some text to encrypt (`exit` to terminate the program)");

        while (true)
        { 
            Console.Write("plain text > ");
            string? plainText = Console.ReadLine();

            if (plainText == null || plainText == "exit")
                return;

            Console.WriteLine();

            using AsymEncryptionHelper encryptionHelper = new(publicKey, privateKey);

            encryptionHelper.AESCallback += (plainAesKey, cipherAesKey, Nonce, Tag) =>
            {
                WriteLabel("AES-GCM 256 Debug Information:");
                
                DisplayKeyValue("Nonce", FormatByteArrayAsString(Nonce));
                DisplayKeyValue("Tag", FormatByteArrayAsString(Tag));
                DisplayKeyValue("Plain AES Key", FormatByteArrayAsString(plainAesKey));
                DisplayKeyValue("Cipher AES Key", FormatByteArrayAsString(cipherAesKey, 16));
                
                Console.WriteLine();
            };

            WriteTitle("Encryption / Decryption Tester");

            string cipherText = encryptionHelper.EncryptToJson(plainText);

            WriteLabel("Ciphertext:");
            Console.WriteLine(cipherText);
            Console.WriteLine();

            byte[] plainTextBuffer = encryptionHelper.DecryptFromJson(cipherText);

            WriteLabel("Plaintext:");
            Console.WriteLine(Encoding.UTF8.GetString(plainTextBuffer));            
            Console.WriteLine();
        }      
    }
}