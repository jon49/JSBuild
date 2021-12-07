// See https://aka.ms/new-console-template for more information

using static JSBuild.Util;
using JSBuild.Utils;

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
    static async Task Main(string path = "", string sw = "", string @out = "")
    {
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

        var files =
            SearchDirectory(
                directory: new DirectoryInfo(path),
                include: new[] { "*.ts", "*.js", "*.css", "*.html" },
                exclude: new[] { ".d.ts" },
                excludedDirectories: new[] { "node_modules" } )
            .Select(x => new FileData(x, swFileName))
            .ToArray();
        await Dependency.SetAsync(files, path);
        var hierarchy = Hierarchy.Get(files);
        //var hash = new Dictionary<string, FileData>();
        //GetHashes(files, hash);

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

