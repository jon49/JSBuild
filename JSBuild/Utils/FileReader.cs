using System.Text;

namespace JSBuild.Utils
{
    internal class FileReader : IDisposable
    {
        private readonly FileData _file;
        private readonly ESBuild? _esbuild;
        private readonly FileStream? _stream;
        private readonly StreamReader _reader;

        public FileReader(FileData file)
        {
            _file = file;
            if (_file.Types.Contains(FileType.TypeScript))
            {
                _esbuild = new ESBuild(file.Path.FullName);
                _reader = _esbuild.Reader;
            }
            else
            {
                _stream = new FileStream(file.Path.FullName, FileMode.Open, FileAccess.Read);
                _reader = new StreamReader(_stream, Encoding.UTF8);
            }
        }

        public Task<string?> ReadLineAsync() => _reader.ReadLineAsync();

        public void Dispose()
        {
            _reader?.Dispose();
            _esbuild?.Dispose();
            _stream?.Dispose();
        }
    }
}
