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

using System.Net;
using System.Text;

/// <summary>
/// The FtpHelper class is a utility in C# designed to streamline the application of the FTP protocol.
/// This is accomplished through abstraction and simplification of the built-in WebRequest class,
/// providing users with a more intuitive and manageable interface for FTP operations.
/// 
/// Supported operations:
///     * Session Actions
///     * Stream Upload (Generic)
///     * File Upload
///     * String Upload
///     * Create Directory
///     * Delete File
///     * Enumerate Directory Files
/// </summary>
public class FtpHelper
{
    public Guid? Session;
    public string Host;
    public string Username;
    private string Password;
    private bool Secure;

    public FtpHelper(string host, string username, string password, bool secure, Guid? session = null)
    {
        this.Host = host;
        this.Username = username;
        this.Password = password;
        this.Session = session;
        this.Secure = secure;
    }

    private FtpWebRequest _NewRequest(string? uri)
    {
#pragma warning disable SYSLIB0014
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{this.Host}/{uri ?? ""}");
#pragma warning restore SYSLIB0014

        request.Credentials = new NetworkCredential(this.Username, this.Password);

        request.UsePassive = true;
        request.UseBinary = true;
        request.KeepAlive = true;
        request.EnableSsl = this.Secure;

        return request;
    }

    private FtpWebRequest _NewSessionRequest(string? uri = null)
    {
        if (this.Session == null)
            return _NewRequest(uri);
        else
            return _NewRequest($"{uri ?? "NULL"}.{this.Session}");
    }

    public void UploadData(Stream data, string destFilePath)
    {
        FtpWebRequest request = this._NewSessionRequest(destFilePath);

        request.Method = WebRequestMethods.Ftp.UploadFile;

        using Stream requestStream = request.GetRequestStream();

        data.CopyTo(requestStream);
    }

    public void UploadFile(string localFilePath, string destFilePath)
    {
        using FileStream fileStream = File.Open(localFilePath, FileMode.Open, FileAccess.Read);

        UploadData(fileStream, destFilePath);
    }

    public void UploadString(string content, string destFilePath)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(content);

        using MemoryStream stream = new(bytes);

        UploadData(stream, destFilePath);
    }

    public Stream DownloadData(string remoteFilePath)
    {
        FtpWebRequest request = this._NewSessionRequest(remoteFilePath);

        request.Method = WebRequestMethods.Ftp.DownloadFile;

        FtpWebResponse response = (FtpWebResponse)request.GetResponse();

        Stream stream = response.GetResponseStream();

        return stream;
    }

    public void DownloadFile(string remoteFilePath, string destinationFilePath)
    {
        using Stream stream = DownloadData(remoteFilePath);

        using FileStream fileStream = new(destinationFilePath, FileMode.Create, FileAccess.Write);

        stream.CopyTo(fileStream);
    }

    public string DownloadString(string remoteFilePath)
    {
        using Stream stream = DownloadData(remoteFilePath);

        using StreamReader reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    private void _ExecuteFTPCommand(string remoteDirectoryPath, string command)
    {
        FtpWebRequest request = this._NewSessionRequest(remoteDirectoryPath);

        request.Method = command;

        using FtpWebResponse resp = (FtpWebResponse)request.GetResponse();
    }

    public void CreateDirectory(string remoteDirectoryPath)
    {
        _ExecuteFTPCommand(remoteDirectoryPath, WebRequestMethods.Ftp.MakeDirectory);
    }

    public void DeleteFile(string remoteDirectoryPath)
    {
        _ExecuteFTPCommand(remoteDirectoryPath, WebRequestMethods.Ftp.DeleteFile);
    }

    private List<string> _ListDirectory(string remoteDirectoryPath = "")
    {
        List<string> items = new();
        try
        {
            FtpWebRequest request = this._NewRequest(remoteDirectoryPath);

            request.Method = WebRequestMethods.Ftp.ListDirectory;

            using FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            using StreamReader reader = new(response.GetResponseStream());

            string? file = "";

            while (true)
            {
                file = reader.ReadLine();

                if (string.IsNullOrEmpty(file))
                    break;

                items.Add(file);
            }
        }
        catch
        { }

        ///
        return items;
    }

    public List<string> ListDirectory(string remoteDirectoryPath = "")
    {
        List<string> items = this._ListDirectory(remoteDirectoryPath);
        if (this.Session == null)
            return items;

        List<string> filteredItems = new();

        foreach (string file in items)
        {
            if (file.EndsWith(this.Session.ToString()))
                // We remove ending session value (including the dot `-1`)
                filteredItems.Add(file.Substring(0, (file.Length - this.Session.ToString().Length) - 1));
        }

        ///
        return filteredItems;
    }

}
