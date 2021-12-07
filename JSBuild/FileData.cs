using System.Text.RegularExpressions;

namespace JSBuild;

internal class FileData {

    public FileData(FileInfo path, string serviceWorkerFilename)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        IsServiceWorker = Path.Name == serviceWorkerFilename;

        Types = new();
        Types.Add(Path.Extension switch
        {
            ".ts" => FileType.TypeScript,
            ".js" => FileType.JavaScript,
            ".html" => FileType.HTML,
            ".css" => FileType.CSS,
            _ => throw new NotSupportedException(Path.FullName),
        });
        if (_isHTMLScript.IsMatch(Path.Name))
        {
            Types.Add(FileType.HTMLScript);
        }
    }

    private static readonly Regex _isHTMLScript = new(@"\.html\.[tj]s", RegexOptions.Compiled);

    public FileInfo Path { get; }
    public string NormalizedName
        => Types.Contains(FileType.TypeScript)
            ? string.Concat(Path.FullName.AsSpan(0, Path.FullName.Length - 2), "js")
        : Path.FullName;
    public List<FileData> Dependencies { get; } = new List<FileData>();
    public string? HashFilename { get; set; }
    public List<FileType> Types { get; }
    public bool IsServiceWorker { get; }
    public bool InHierarchy { get; set; } = false;
    public override string ToString() => $"{Path.FullName} <> {Types} <> IsServiceWorker {IsServiceWorker}";

    public override int GetHashCode()
        => Path.GetHashCode();
}

internal enum FileType
{
    TypeScript,
    JavaScript,
    HTML,
    CSS,
    HTMLScript,
}
