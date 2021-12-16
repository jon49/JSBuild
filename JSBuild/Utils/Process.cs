using System.ComponentModel;

namespace JSBuild.Utils;
internal static class Process
{
    public static string? GetExecutableFullPath(string exeName)
    {
        try
        {
            using var p = new System.Diagnostics.Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = "where";
            p.StartInfo.Arguments = exeName;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                return null;
            }

            // just return first match
            return output[..output.IndexOf(Environment.NewLine)];
        }
        catch(Win32Exception)
        {
            throw new Exception("'where' command is not on path");
        }
    }

}
