using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RelinkModOrganizer.Helpers;
public static class Md5Helper
{
    public static string CalculateMd5(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return BitConverter
            .ToString(hash)
            .Replace("-", string.Empty)
            .ToLowerInvariant();
    }
}
