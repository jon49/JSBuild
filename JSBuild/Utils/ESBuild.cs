using JSBuild.Exceptions;
using System.Diagnostics;

namespace JSBuild.Utils
{
    internal class ESBuild : IDisposable
    {
        private static readonly string _esbuildPath =
            Process.GetExecutableFullPath("esbuild.cmd") ?? throw new FileNotFoundException("Esbuild is not available!");
        private readonly ProcessStartInfo _startInfo;
        private readonly System.Diagnostics.Process _process;

        public ESBuild(string filename)
        {
            _startInfo = new ProcessStartInfo
            {
                FileName = _esbuildPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = $"{filename} --target=es2021",
            };
            _process =
                System.Diagnostics.Process.Start(_startInfo)
                ?? throw new StartProcessException("Could not start ESBuild process.");
        }

        public StreamReader Reader
            => _process.StandardOutput
            ?? throw new ReaderException();

        public void Dispose()
        {
            _process.Dispose();
        }
    }
}
