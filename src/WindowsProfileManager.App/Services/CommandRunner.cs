using System.Diagnostics;
using System.Text;

namespace WindowsProfileManager.App.Services;

public sealed class CommandRunner
{
    public string Run(string fileName, string arguments, int timeoutMilliseconds = 15000)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        if (!process.WaitForExit(timeoutMilliseconds))
        {
            try { process.Kill(); } catch { }
            return $"Tempo esgotado ao executar {fileName} {arguments}.";
        }

        return string.IsNullOrWhiteSpace(error) ? output : $"{output}{Environment.NewLine}{error}".Trim();
    }
}
