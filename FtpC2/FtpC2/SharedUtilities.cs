using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


internal class SharedUtilities
{
    public static Guid ComputeFingerprintAsGuid(byte[] data)
    {
        using MD5 md5 = MD5.Create();

        byte[] hash = md5.ComputeHash(data);

        return new Guid(hash);
    }
}

