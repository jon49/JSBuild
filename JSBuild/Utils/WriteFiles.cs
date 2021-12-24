namespace JSBuild.Utils;
internal static class WriteFiles
{
    public static ParallelLoopResult Start(Dictionary<string, FileData> files, string outDir)
    {
        return Parallel.ForEach(files.Values, (file, _) =>
        {
            var targetFilename = GetOutFilename(file, outDir);

            if (File.Exists(targetFilename))
            {
                return;
            }

            if (file.TempPath is { })
            {
                File.Move(file.TempPath, targetFilename);
            }
            else
            {
                File.Copy(file.Path.FullName, targetFilename);
            }
        });
    }

    public static string GetOutFilename(FileData file, string outDir)
    {
        var targetDirectory = Path.Combine(outDir, file.RelativeDirectory);
        Directory.CreateDirectory(targetDirectory);
        var targetFilename = Path.Combine(targetDirectory, file.HashedUrl[(file.HashedUrl.LastIndexOf('/') + 1)..]);
        return targetFilename;
    }
}
