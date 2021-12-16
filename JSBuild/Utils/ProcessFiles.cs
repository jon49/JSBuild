namespace JSBuild.Utils;
internal static class ProcessFiles
{
    public static async Task StartAsync(List<List<FileData>> hierarchy, Dictionary<string, FileData> files, string root)
    {
        foreach (var list in hierarchy)
        {
            await Parallel.ForEachAsync(list.Where(x => !x.IsServiceWorker), async (f, _) =>
            {
                using var p = new ProcessFile(f, files, root);
                await p.StartAsync();
            });
        }
        var serviceWorker = files.Values.FirstOrDefault(x => x.IsServiceWorker);
        if (serviceWorker is { })
        {
            using var p = new ProcessFile(serviceWorker, files, root);
            await p.StartAsync();
        }
    }
}
