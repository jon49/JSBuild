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
        RelativeDirectory = System.IO.Path.GetRelativePath(
            Environment.CurrentDirectory,
            System.IO.Path.GetDirectoryName(Path.FullName) ?? ".");
        NormalizedName =
            Types.Contains(FileType.TypeScript)
                ? string.Concat(Path.FullName.AsSpan(0, Path.FullName.Length - 2), "js")
            : Path.FullName;
        Url = $"/{System.IO.Path.GetRelativePath(Environment.CurrentDirectory, NormalizedName).Replace('\\', '/')}";
    }

    private static readonly Regex _isHTMLScript = new(@"\.html\.[tj]s", RegexOptions.Compiled);

    public FileInfo Path { get; }
    public string NormalizedName { get; }
    public string RelativeDirectory { get; }
    public List<FileData> Dependencies { get; } = new List<FileData>();
    public string Url { get; }
    public string? TempPath { get; set; }
    public string? Hash { get; set; }
    public List<FileType> Types { get; }
    public bool IsServiceWorker { get; }
    public bool InHierarchy { get; set; } = false;
    private string? _hashUrl;
    public string HashedUrl
    {
        get
        {
            if (_hashUrl is { })
            {
                return _hashUrl;
            }

            var isHTML = Types.Contains(FileType.HTML);
            if (!isHTML && Hash is { } && !IsServiceWorker)
            {
                var lastPeriod = Url.LastIndexOf(".");
                _hashUrl = $"{Url[..lastPeriod]}.{Hash[(Hash.Length - 8)..].ToLower()}{Url[lastPeriod..]}";
            }
            else
            {
                _hashUrl = Url;
            }
            return _hashUrl;
        }
    }

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
