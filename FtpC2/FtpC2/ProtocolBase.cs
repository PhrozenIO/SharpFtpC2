using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

internal class ProtocolBase
{
    private readonly FtpHelper FTP;

    public delegate string DataModifierDelegate(string data);

    public DataModifierDelegate? IngressDataModifier;
    public DataModifierDelegate? EgressDataModifier;

    public ProtocolBase(string host, string username, string password, bool secure, Guid? session = null)
    {
        this.FTP = new(host, username, password, secure, session);
    }

    public Guid? GetSession()
    {
        return this.FTP.Session;
    }

    public void UploadString(string content, string destFilePath)
    {
        content = EgressDataModifier != null ? EgressDataModifier.Invoke(content) : content;

        this.FTP.UploadString(content, destFilePath);
    }

    public string DownloadString(string remoteFilePath)
    {
        string content = this.FTP.DownloadString(remoteFilePath);

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
}