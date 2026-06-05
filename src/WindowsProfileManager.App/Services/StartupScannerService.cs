using Microsoft.Win32;
using WindowsProfileManager.App.Models;

namespace WindowsProfileManager.App.Services;

public sealed class StartupScannerService
{
    public IReadOnlyList<StartupItem> Scan(IReadOnlyList<ServiceItem> services)
    {
        var items = new List<StartupItem>();
        ReadRunKey(items, Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", "HKCU Run");
        ReadRunKey(items, Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run", "HKLM Run");
        ReadStartupFolder(items, Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Startup usuario");
        ReadStartupFolder(items, Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "Startup global");

        items.AddRange(services
            .Where(s => s.StartupType.Equals("Automatico", StringComparison.OrdinalIgnoreCase))
            .Select(s => new StartupItem
            {
                Name = s.DisplayName,
                Source = "Servico automatico",
                Enabled = true,
                Command = s.ExecutablePath,
                Allowed = true,
                IsProtected = s.IsProtected,
                Category = s.IsProtected ? "Protegido" : "Servico"
            }));

        return items.OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static void ReadRunKey(List<StartupItem> items, RegistryKey root, string subKey, string source)
    {
        try
        {
            using var key = root.OpenSubKey(subKey);
            if (key is null)
            {
                return;
            }

            foreach (var name in key.GetValueNames())
            {
                items.Add(new StartupItem
                {
                    Name = name,
                    Source = source,
                    Enabled = true,
                    Command = Convert.ToString(key.GetValue(name)) ?? "",
                    Allowed = true
                });
            }
        }
        catch
        {
        }
    }

    private static void ReadStartupFolder(List<StartupItem> items, string folder, string source)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(folder))
        {
            items.Add(new StartupItem
            {
                Name = System.IO.Path.GetFileName(file),
                Source = source,
                Enabled = true,
                Command = file,
                Allowed = true
            });
        }
    }
}
