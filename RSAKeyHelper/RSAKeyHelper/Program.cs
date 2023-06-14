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

class Program
{
    public const int KEYLENGTH = 4096;

    public static void WriteTitle(string banner)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"@{banner}");
        Console.ResetColor();
    }

    public static void Main(string[] args)
    {
        using RSACryptoServiceProvider rsa = new(KEYLENGTH);

        byte[] privateKey = rsa.ExportRSAPrivateKey();
        byte[] publicKey = rsa.ExportRSAPublicKey();

        WriteTitle("PubKey:");
        Console.WriteLine(Convert.ToBase64String(publicKey));
        WriteTitle("EOF");

        Console.WriteLine();

        WriteTitle("PrivKey:");
        Console.WriteLine(Convert.ToBase64String(privateKey));
        WriteTitle("EOF");
    }
}