using System.Text.RegularExpressions;

namespace JSBuild.Utils;
internal static class Dependency
{

    public static Task SetAsync(FileData[] files, string root)
        => Parallel.ForEachAsync(files, async (file, _) =>
        {
            var readScript = file.Types.Contains(FileType.JavaScript) || file.Types.Contains(FileType.TypeScript);
            var readHTML = file.Types.Contains(FileType.HTML) || file.Types.Contains(FileType.HTMLScript);
            using var reader = File.OpenText(file.Path.FullName);
            string? line;
            var lineNumber = 0;
            var foundImport = false;
            var directory =
                Path.GetDirectoryName(file.Path.FullName)
                ?? throw new DirectoryNotFoundException(file.Path.FullName);
            while ((line = await reader.ReadLineAsync()) is { })
            {
                if (line is null)
                {
                    break;
                }

                lineNumber++;
                if (readScript)
                {
                    var result = GetScriptDependency(line);
                    readScript = !((result is null && foundImport) || (result is null && lineNumber == 10));
                    if (result is { })
                    {
                        var filename = GetFilename(root, directory, result);
                        if (filename is { })
                        {
                            file.Dependencies.Add(files.First(f => f.NormalizedName == filename));
                        }
                    }
                }
                if (readHTML)
                {
                    var result = GetHTMLDepedency(line);
                    if (result is { })
                    {
                        var filename = GetFilename(root, directory, result);
                        if (filename is { })
                        {
                            var data = files.FirstOrDefault(f => f.NormalizedName == filename);
                            if (data is { })
                            {
                                file.Dependencies.Add(data);
                            }
                        }
                    }
                }
                if (!(readHTML || readScript))
                {
                    break;
                }
            }
        });

    private static string? GetFilename(string root, string directory, string path)
    {
        var filename =
            new Uri(path.StartsWith(".")
                ? Path.Join(directory, path)
            : Path.Join(root, "." + path)).LocalPath;
        var name = Path.GetFileName(filename);
        return !name.Contains('.')
            ? null
            : filename;
    }

    private static readonly Regex importDependency = new(@"from +(""|')([^""^']+)\1", RegexOptions.Compiled);
    private static string? GetScriptDependency(string line)
    {
        var match = importDependency.Match(line).Groups[2].Value;
        return
            match.EndsWith(".js")
                ? match
            : null;
    }

    private static readonly Regex htmlDepedency = new(@"(src=|href=)(""|')([^""^']+)\2", RegexOptions.Compiled);
    private static string? GetHTMLDepedency(string line)
    {
        var match = htmlDepedency.Match(line).Groups[3].Value;
        return match.Length > 0 ? match : null;
    }

}
