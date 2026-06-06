using System.Diagnostics;
using WindowsProfileManager.App.Models;

namespace WindowsProfileManager.App.Services;

public sealed class ProcessScannerService
{
    private readonly ProtectionCatalog _protectionCatalog;

    public ProcessScannerService(ProtectionCatalog protectionCatalog)
    {
        _protectionCatalog = protectionCatalog;
    }

    public IReadOnlyList<ProcessItem> Scan()
    {
        var items = new List<ProcessItem>();

        foreach (var process in Process.GetProcesses())
        {
            try
            {
                items.Add(CreateItem(process));
            }
            catch
            {
                // Processes can exit while Windows is enumerating them. Ignore the vanished item.
            }
            finally
            {
                process.Dispose();
            }
        }

        return items
            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(p => p.Pid)
            .ToList();
    }

    private ProcessItem CreateItem(Process process)
    {
        var pid = SafeRead(() => process.Id);
        var name = SafeRead(() => process.ProcessName) ?? "";
        if (!name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && pid > 4)
        {
            name += ".exe";
        }

        var item = new ProcessItem
        {
            Name = name,
            Pid = pid,
            CpuSeconds = SafeRead(() => process.TotalProcessorTime.TotalSeconds),
            RamBytes = SafeRead(() => process.WorkingSet64),
            Path = SafeRead(() => process.MainModule?.FileName ?? ""),
            User = "",
            Allowed = true
        };

        item.IsProtected = _protectionCatalog.IsProtectedProcess(item.Name, item.Pid)
            || string.Equals(item.Path, Environment.ProcessPath, StringComparison.OrdinalIgnoreCase);
        item.Category = item.IsProtected ? "Protegido" : GuessCategory(item.Path);
        return item;
    }

    private static string GuessCategory(string path)
    {
        if (path.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Windows), StringComparison.OrdinalIgnoreCase))
        {
            return "Windows";
        }

        return string.IsNullOrWhiteSpace(path) ? "Desconhecido" : "Aplicativo";
    }

    private static T SafeRead<T>(Func<T> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return default!;
        }
    }
}
