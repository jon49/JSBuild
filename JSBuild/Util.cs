using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.IO.Pipes;

namespace JSBuild;

internal static class Util
{
    public static List<FileInfo> SearchDirectory(
        DirectoryInfo directory,
        string[] include,
        string[] exclude,
        string[] excludedDirectories,
        List<FileInfo>? files = null)
    {
        if (files is null)
        {
            files = new List<FileInfo>();
        }
        files.AddRange(
            include
                .SelectMany(x => directory.GetFiles(x))
                .Where(x => !exclude.Any(y => x.Name.EndsWith(y))) );

        foreach (var dir in directory.GetDirectories())
        {
            if (!excludedDirectories.Contains(dir.Name))
            {
                SearchDirectory(dir, include, exclude, excludedDirectories, files);
            }
        }

        return files;
    }

    //public static void GetHashes(FileData[] files, Dictionary<string, FileData> hash)
    //{
    //    var count = 0;
    //    while (files.Length != hash.Count || count != 100)
    //    {
    //        foreach (var file in files)
    //        {
    //            if (!hash.ContainsKey(file.NormalizedName)
    //                && (file.Dependencies.Count == 0
    //                    || (file.Dependencies.Count > 0
    //                        && file.Dependencies.All(x => hash.ContainsKey(NormalizeName(x.Path.FullName))) )))
    //            {
    //                SetFileHash(file, hash);
    //                hash.Add(file.NormalizedName, file);
    //            }
    //        }
    //        count++;
    //    }
    //}

    private static string NormalizeName(string value)
        => value.EndsWith(".ts") ? string.Concat(value.AsSpan(0, -2), "js") : value;

    //private static readonly SHA256 sha256 = SHA256.Create();
    //private static readonly UTF8Encoding encoding = new();
    //private static void SetFileHash(FileData file, Dictionary<string, FileData> hash)
    //{
    //    var server = new AnonymousPipeServerStream();
    //    AnonymousPipeClientStream client = new(server.GetClientHandleAsString());

    //    using var stream = file.Path.OpenRead();
    //    using var reader = new StreamReader(stream);

    //    var hashValue = sha256.ComputeHash(client);

    //    string? line;
    //    while ((line = reader.ReadLine()) is { })
    //    {
    //        if (line.StartsWith("import"))
    //        {
    //            line = _firstString.Replace(line, "");
    //        }
    //        server.Write(encoding.GetBytes(line));
    //    }
    //    Console.WriteLine($"HASH: {string.Join("", hashValue.Select(x => x.ToString("x2")))}");
    //}
}
