using Microsoft.Win32;
using WindowsProfileManager.App.Models;

namespace WindowsProfileManager.App.Services;

public sealed class StartupControlService
{
    public string Disable(StartupItem item)
    {
        if (item.IsProtected)
        {
            return $"Bloqueado: {item.Name} e protegido.";
        }

        try
        {
            if (item.Source.Equals("HKCU Run", StringComparison.OrdinalIgnoreCase))
            {
                return DeleteRunValue(Registry.CurrentUser, item);
            }

            if (item.Source.Equals("HKLM Run", StringComparison.OrdinalIgnoreCase))
            {
                return DeleteRunValue(Registry.LocalMachine, item);
            }

            if (item.Source.StartsWith("Startup", StringComparison.OrdinalIgnoreCase) && File.Exists(item.Command))
            {
                var disabledPath = item.Command + ".disabled";
                File.Move(item.Command, disabledPath, true);
                return $"OK: item movido para {disabledPath}.";
            }

            return $"Nao suportado automaticamente: {item.Name} ({item.Source}).";
        }
        catch (Exception ex)
        {
            return $"Falha ao desativar {item.Name}: {ex.Message}";
        }
    }

    private static string DeleteRunValue(RegistryKey root, StartupItem item)
    {
        using var key = root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
        key?.DeleteValue(item.Name, throwOnMissingValue: false);
        return $"OK: item removido de {item.Source}: {item.Name}.";
    }
}
