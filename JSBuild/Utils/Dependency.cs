namespace JSBuild.Utils;
internal static class Dependency
{

    public static Task SetAsync(Dictionary<string, FileData> files, string root)
        => Parallel.ForEachAsync(files.Values, async (file, _) =>
        {
            var processor = new DependencyProcessor<FileData>(file, files, root, (f, _, _) => f);
            using var reader = File.OpenText(file.Path.FullName);
            string? line;
            while ((line = await reader.ReadLineAsync()) is { })
            {
                if (line is null)
                {
                    break;
                }

                var dependency = processor.ProcessLine(line);
                if (dependency is { })
                {
                    file.Dependencies.Add(dependency);
                }

                if (processor.FoundAllDependencies)
                {
                    break;
                }
            }
        });

}
