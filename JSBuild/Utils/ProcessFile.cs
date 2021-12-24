using Nerdbank.Streams;
using System.Text;
using System.Security.Cryptography;

namespace JSBuild.Utils;

internal class ProcessFile : IDisposable
{
    private readonly FileData _file;
    private readonly Dictionary<string, FileData> _files;
    private readonly OutBehavior _outBehavior;
    private readonly bool _shouldUpdateDependencies;
    private readonly DependencyProcessor<string>? _processor;
    private readonly string _outDir;

    public ProcessFile(FileData file, Dictionary<string, FileData> files, string outDir)
    {
        _outDir = outDir;
        _file = file;
        _files = files;
        _shouldUpdateDependencies = file.Dependencies.Any();
        if (_shouldUpdateDependencies)
        {
            _processor = new DependencyProcessor<string>(
                file,
                files,
                outDir,
                (f, line, match) => line.Replace(match, f.HashedUrl));
        }
        _outBehavior =
            file.Types.Contains(FileType.JavaScript) || file.Types.Contains(FileType.TypeScript)
                ? OutBehavior.IntermediateWrite
            : OutBehavior.CopyOnly;
    }

    public static readonly string TempPath = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());
    private FileReader? _fileReader;
    private SimplexStream? _streamGetHash;
    private StreamWriter? _writer;
    private SimplexStream? _streamWriteTempFile;
    private StreamWriter? _writer2;
    private StreamReader? _fileWriterReader;
    public Task StartAsync()
    {
        _streamGetHash = new SimplexStream();

        if (_outBehavior == OutBehavior.IntermediateWrite)
        {
            _streamWriteTempFile = new SimplexStream();
        }

        return Task.WhenAll(
            WriteStreamsAsync(_streamGetHash, _streamWriteTempFile),
            SetHashAsync(_streamGetHash),
            WriteTempFileAsync(_streamWriteTempFile));
    }

    private Task WriteStreamsAsync(SimplexStream stream, SimplexStream? stream2)
        => Task.Run(async delegate
        {
            _fileReader = new FileReader(_file);
            _writer = new StreamWriter(stream);
            if (stream2 is { }) _writer2 = new StreamWriter(stream2);

            var searchingServiceWorkerList = _file.IsServiceWorker;
            string? line;
            while ((line = await _fileReader.ReadLineAsync()) is { })
            {
                if (_shouldUpdateDependencies && _processor is { } && !_processor.FoundAllDependencies)
                {
                    var newLine = _processor.ProcessLine(line);
                    line = newLine is { } ? newLine : line;
                }
                else if (searchingServiceWorkerList && line.StartsWith("const links"))
                {
                    var urls = string.Join(
                        @""",""",
                        _files.Values
                        .Where(x => !x.IsServiceWorker)
                        .Select(x => x.HashedUrl));
                    line = $@"const links = [""{urls}""];";
                    searchingServiceWorkerList = false;
                }
                await _writer.WriteLineAsync(line);
                await _writer.FlushAsync();
                if (_writer2 is { })
                {
                    await _writer2.WriteLineAsync(line);
                    await _writer2.FlushAsync();
                }
            }
            stream.CompleteWriting();
            if (_streamWriteTempFile is { }) _streamWriteTempFile.CompleteWriting();
        });

    private Task SetHashAsync(SimplexStream stream)
        => Task.Run(async delegate
        {
            using var sha256 = SHA256.Create();
            var hash = await sha256.ComputeHashAsync(stream);
            _file.Hash = BitConverter.ToString(hash).Replace("-", "");
        });

    private Task WriteTempFileAsync(SimplexStream? stream)
        => stream is { }
            ? Task.Run(async delegate
            {
                var reader = _fileWriterReader = new StreamReader(stream);
                if (_file.IsServiceWorker)
                {
                    _file.TempPath = WriteFiles.GetOutFilename(_file, _outDir);
                }
                else
                {
                    _file.TempPath = Path.Combine(TempPath, "." + _file.Url);
                }
                Directory.CreateDirectory(Path.GetDirectoryName(_file.TempPath)!);
                using var fs = new FileStream(_file.TempPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                string? line;
                while ((line = await reader.ReadLineAsync()) is { })
                {
                    await fs.WriteAsync(Encoding.UTF8.GetBytes(line));
                }
            })
        : Task.CompletedTask;

    public void Dispose()
    {
        _fileReader?.Dispose();
        _streamGetHash?.Dispose();
        _writer?.Dispose();
        _streamWriteTempFile?.Dispose();
        _writer2?.Dispose();
        _fileWriterReader?.Dispose();
    }
}

internal enum OutBehavior
{
    CopyOnly,
    IntermediateWrite,
}

