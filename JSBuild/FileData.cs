using JSBuild.Utils;
using System.Text.RegularExpressions;

namespace JSBuild;

internal class FileData {

    private static readonly Regex _isHTMLScript = new(@"\.html\.[tj]s", RegexOptions.Compiled);
    private readonly string root;
    private readonly string outDir;
    public static readonly string TempPath = System.IO.Path.Join(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());

    public FileData(FileInfo path, string serviceWorkerFilename, string root, string outDir)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        this.root = root ?? throw new ArgumentNullException(nameof(root));
        this.outDir = outDir ?? throw new ArgumentNullException(nameof(outDir));
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

    public FileInfo Path { get; }
    public string NormalizedName { get; }
    public string RelativeDirectory { get; }
    public List<FileData> Dependencies { get; } = new List<FileData>();
    public string Url { get; }

    private string? _outFilename;
    public string OutFilename
    {
        get
        {
            if (_outFilename is null)
            {
                var targetDirectory = System.IO.Path.Combine(outDir, RelativeDirectory);
                Directory.CreateDirectory(targetDirectory);
                _outFilename = System.IO.Path.Combine(targetDirectory, HashedUrl[(HashedUrl.LastIndexOf('/') + 1)..]);
            }
            return _outFilename;
        }
    }

    private string? _tempFilename;
    public string TempFilename
    {
        get
        {
            if (_tempFilename is null)
            {
                _tempFilename =
                    IsServiceWorker
                        ? OutFilename
                    : System.IO.Path.Combine(TempPath, "." + Url);
            }
            return _tempFilename;
        }
    }
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
