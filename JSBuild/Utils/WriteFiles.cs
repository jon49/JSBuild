namespace JSBuild.Utils;
internal static class WriteFiles
{
    public static ParallelLoopResult Start(Dictionary<string, FileData> files)
    {
        return Parallel.ForEach(files.Values, (file, _) =>
        {
            var targetFilename = file.OutFilename;

            if (File.Exists(targetFilename))
            {
                return;
            }

            if (File.Exists(file.TempFilename))
            {
                File.Move(file.TempFilename, targetFilename);
            }
            else
            {
                File.Copy(file.Path.FullName, targetFilename);
            }
        });
    }
}
