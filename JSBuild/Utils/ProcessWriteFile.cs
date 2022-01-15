using Nerdbank.Streams;
using System.Text;

namespace JSBuild.Utils;

internal static class ProcessWriteFile
{
    public static Task StartAsync(SimplexStream? stream, string targetFilename, bool devEnvironment)
        => stream is { }
            ? Task.Run(async delegate
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetFilename)!);
                using var fs = new FileStream(targetFilename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                using var reader = new StreamReader(stream);
                string? line;
                while ((line = await reader.ReadLineAsync()) is { })
                {
                    if (devEnvironment)
                    {
                        line += "\n";
                    }
                    await fs.WriteAsync(Encoding.UTF8.GetBytes(line));
                }
            })
        : Task.CompletedTask;
}
