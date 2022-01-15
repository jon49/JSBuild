using Nerdbank.Streams;
namespace JSBuild.Utils;

internal class ProcessFile : IAsyncDisposable
{
    private readonly FileData _file;
    private readonly Dictionary<string, FileData> _files;
    private readonly OutBehavior _outBehavior;
    private readonly bool _shouldUpdateDependencies;
    private readonly DependencyProcessor<string>? _processor;
    private readonly bool _dev;

    public ProcessFile(FileData file, Dictionary<string, FileData> files, string outDir, bool dev)
    {
        _dev = dev;
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

    SimplexStream? writerStream = null;
    StreamWriter? writer = null;
    StreamWriter? writer2 = null;
    SimplexStream? hashStream = null;
    FileReader? fileReader = null;
    public Task StartAsync()
    {
        hashStream = new SimplexStream();

        if (_outBehavior == OutBehavior.IntermediateWrite)
        {
            writerStream = new SimplexStream();
        }

        return Task.WhenAll(
            WriteStreamsAsync(hashStream, writerStream),
            ProcessHash.StartAsync(hashStream, _file),
            ProcessWriteFile.StartAsync(writerStream, _file.TempFilename, _dev));
    }

    private Task WriteStreamsAsync(SimplexStream stream, SimplexStream? writerStream)
        => Task.Run(async delegate
        {
            fileReader = new FileReader(_file);
            writer = new StreamWriter(stream);
            if (writerStream is { })
            {
                writer2 = new StreamWriter(writerStream);
            }

            var searchingServiceWorkerList = _file.IsServiceWorker;
            string? line;
            while ((line = await fileReader.ReadLineAsync()) is { })
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

                await writer.WriteLineAsync(line);
                await writer.FlushAsync();
                if (writer2 is { })
                {
                    await writer2.WriteLineAsync(line);
                    await writer2.FlushAsync();
                }
            }
            stream.CompleteWriting();
            if (writerStream is { }) writerStream.CompleteWriting();
        });

    public async ValueTask DisposeAsync()
    {
        if (writer2 is { })
        {
            await writer2.WriteLineAsync();
        }
        if (writerStream is { })
        {
            await writerStream.DisposeAsync();
        }
        if (hashStream is { })
        {
            await hashStream.DisposeAsync();
        }
        if (fileReader is { })
        {
            fileReader.Dispose();
        }
        if (writer is { })
        {
            await writer.DisposeAsync();
        }
    }
}

internal enum OutBehavior
{
    CopyOnly,
    IntermediateWrite,
}

