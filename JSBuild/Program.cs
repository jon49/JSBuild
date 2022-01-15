// See https://aka.ms/new-console-template for more information

using static JSBuild.Util;
using JSBuild.Utils;
using System.Diagnostics;

namespace JSBuild;

class Program
{
    // https://github.com/dotnet/command-line-api/blob/main/docs/Your-first-app-with-System-CommandLine-DragonFruit.md
    // https://docs.microsoft.com/en-us/dotnet/api/system.io.filestream?view=net-6.0
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path">Path to directory with source files.</param>
    /// <param name="sw">Service Worker filename.</param>
    /// <param name="out">Output directory.</param>
    /// <param name="dev">Developer build</param>
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
    static async Task Main(string path = "", string sw = "", string @out = "", bool dev = false)
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        if (@out == string.Empty) @out = "./public";
        @out = Path.GetFullPath(@out);
        Directory.CreateDirectory(@out);

        var cwd = Environment.CurrentDirectory;

        string swFileName = string.Empty;
        if (sw != string.Empty) swFileName = Path.GetFileName(sw);

        path =
            path.Length > 0
                ? Path.GetFullPath(path)
            : Environment.CurrentDirectory;
        Environment.CurrentDirectory = path;

        var setupTime = stopwatch.ElapsedMilliseconds;

        var files =
            SearchDirectory(
                directory: new DirectoryInfo(path),
                include: new[] { "*.ts", "*.js", "*.css", "*.html" },
                exclude: new[] { ".d.ts" },
                excludedDirectories: new[] { "node_modules" })
            .Select(x => new FileData(path: x, serviceWorkerFilename: swFileName, root: path, outDir: @out))
            .ToDictionary(f => f.NormalizedName);

        var directorySearchTime = stopwatch.ElapsedMilliseconds - setupTime;

        await Dependency.SetAsync(files, path);

        var setDependenciesTime = stopwatch.ElapsedMilliseconds - directorySearchTime;

        var hierarchy = Hierarchy.Get(files.Values.ToArray());

        var getHierarchyTime = stopwatch.ElapsedMilliseconds - setDependenciesTime;

        await ProcessFiles.StartAsync(hierarchy, files, path, dev);

        var processFilesTime = stopwatch.ElapsedMilliseconds - getHierarchyTime;

        WriteFiles.Start(files);

        var moveFilesTime = stopwatch.ElapsedMilliseconds - processFilesTime;
        var totalTime = stopwatch.ElapsedMilliseconds;

        Console.WriteLine($"Setup: {setupTime}");
        Console.WriteLine($"Directory Search: {directorySearchTime}");
        Console.WriteLine($"Determine Dependencies: {setDependenciesTime}");
        Console.WriteLine($"Hierarchy: {getHierarchyTime}");
        Console.WriteLine($"Process Hash: {processFilesTime}");
        Console.WriteLine($"Move Files: {moveFilesTime}");
        Console.WriteLine($"Total: {totalTime}");
        Console.WriteLine(FileData.TempPath);

        var depth = 0;
        foreach (var level in hierarchy)
        {
            var spaces = new string(' ', depth * 3);
            foreach(var file in level)
            {
                Console.WriteLine($"{spaces}{file.Path}");
            }
            depth++;
        }

        Environment.CurrentDirectory = cwd;
    }

}

