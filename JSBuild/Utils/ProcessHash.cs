using Nerdbank.Streams;
using System.Security.Cryptography;

namespace JSBuild.Utils;

internal static class ProcessHash
{
    public static Task StartAsync(SimplexStream stream, FileData file)
        => Task.Run(async delegate
        {
            using var sha256 = SHA256.Create();
            var hash = await sha256.ComputeHashAsync(stream);
            file.Hash = BitConverter.ToString(hash).Replace("-", "");
        });
}
