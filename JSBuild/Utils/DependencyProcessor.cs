using System.Text.RegularExpressions;

namespace JSBuild.Utils;

internal class DependencyProcessor<T>
{
    private readonly Dictionary<string, FileData> _files;
    private readonly string _root;
    private readonly Func<FileData, string, string, T?> _action;
    private bool _readScript;
    private readonly bool _readHTML;
    private readonly string _directory;
    private int _lineNumber;
    private bool _foundImport;

    public bool FoundAllDependencies => !(_readHTML || _readScript);

    public DependencyProcessor(FileData file, Dictionary<string, FileData> files, string root, Func<FileData, string, string, T> action)
    {
        _files = files;
        _root = root;
        _action = action;
        _readScript = file.Types.Contains(FileType.JavaScript) || file.Types.Contains(FileType.TypeScript);
        _readHTML = file.Types.Contains(FileType.HTML) || file.Types.Contains(FileType.HTMLScript);
        _directory =
            Path.GetDirectoryName(file.Path.FullName)
            ?? throw new DirectoryNotFoundException(file.Path.FullName);
        _lineNumber = 0;
        _foundImport = false;
    }

    public T? ProcessLine(string? line)
    {
        if (line is null) return default;

        _lineNumber++;
        if (_readScript)
        {
            var result = GetScriptDependency(line);
            _readScript = !((result is null && _foundImport) || (result is null && _lineNumber > 10));
            if (result is { })
            {
                var filename = GetFilename(_root, _directory, result);
                if (filename is { })
                {
                    _foundImport = true;
                    return _action(_files[filename], line, result);
                }
            }
        }
        if (_readHTML)
        {
            var result = GetHTMLDepedency(line);
            if (result is { })
            {
                var filename = GetFilename(_root, _directory, result);
                if (filename is { } && _files.TryGetValue(filename, out FileData? data))
                {
                    return _action(data, line, result);
                }
            }
        }

        return default;
    }

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
