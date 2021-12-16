namespace JSBuild.Utils;
internal static class CopyFiles
{
    public static ParallelLoopResult Start(Dictionary<string, FileData> files, string @out)
    {
        return Parallel.ForEach(files.Values, (file, _) =>
        {
            var targetDirectory = Path.Combine(@out, file.RelativeDirectory);
            Directory.CreateDirectory(targetDirectory);
            var targetFilename = Path.Combine(targetDirectory, file.HashedUrl[(file.HashedUrl.LastIndexOf('/') + 1)..]);

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
}
