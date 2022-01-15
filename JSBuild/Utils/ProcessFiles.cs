namespace JSBuild.Utils;
internal static class ProcessFiles
{
    public static async Task StartAsync(
        List<List<FileData>> hierarchy,
        Dictionary<string, FileData> files,
        string root,
        bool dev)
    {
        foreach (var list in hierarchy)
        {
            await Parallel.ForEachAsync(list.Where(x => !x.IsServiceWorker), async (f, _) =>
            {
                await using var p = new ProcessFile(f, files, root, dev);
                await p.StartAsync();
            });
        }
        var serviceWorker = files.Values.FirstOrDefault(x => x.IsServiceWorker);
        if (serviceWorker is { })
        {
            await using var p = new ProcessFile(serviceWorker, files, root, dev);
            await p.StartAsync();
        }
    }
}
