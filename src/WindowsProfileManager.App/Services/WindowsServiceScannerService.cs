using Microsoft.Win32;
using WindowsProfileManager.App.Models;

namespace WindowsProfileManager.App.Services;

public sealed class WindowsServiceScannerService
{
    private readonly CommandRunner _commandRunner;
    private readonly ProtectionCatalog _protectionCatalog;

    public WindowsServiceScannerService(CommandRunner commandRunner, ProtectionCatalog protectionCatalog)
    {
        _commandRunner = commandRunner;
        _protectionCatalog = protectionCatalog;
    }

    public IReadOnlyList<ServiceItem> Scan()
    {
        var output = _commandRunner.Run("sc.exe", "queryex state= all");
        var services = ParseScQuery(output);

        foreach (var service in services)
        {
            FillRegistryDetails(service);
            service.IsProtected = _protectionCatalog.IsProtectedService(service.Name);
            service.Category = service.IsProtected ? "Protegido" : "Servico";
            service.Allowed = true;
        }

        return services.OrderBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static List<ServiceItem> ParseScQuery(string output)
    {
        var result = new List<ServiceItem>();
        ServiceItem? current = null;

        foreach (var rawLine in output.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            var line = rawLine.Trim();
            if (line.StartsWith("SERVICE_NAME:", StringComparison.OrdinalIgnoreCase))
            {
                current = new ServiceItem { Name = line["SERVICE_NAME:".Length..].Trim() };
                result.Add(current);
                continue;
            }

            if (current is null)
            {
                continue;
            }

            if (line.StartsWith("DISPLAY_NAME:", StringComparison.OrdinalIgnoreCase))
            {
                current.DisplayName = line["DISPLAY_NAME:".Length..].Trim();
            }
            else if (line.StartsWith("STATE", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split(':', 2);
                current.Status = parts.Length == 2 ? parts[1].Trim() : line;
            }
        }

        foreach (var service in result.Where(s => string.IsNullOrWhiteSpace(s.DisplayName)))
        {
            service.DisplayName = service.Name;
        }

        return result;
    }

    private static void FillRegistryDetails(ServiceItem service)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{service.Name}");
            if (key is null)
            {
                return;
            }

            service.ExecutablePath = Convert.ToString(key.GetValue("ImagePath")) ?? "";
            service.StartupType = Convert.ToInt32(key.GetValue("Start", 3)) switch
            {
                2 => "Automatico",
                3 => "Manual",
                4 => "Desativado",
                _ => "Outro"
            };
        }
        catch
        {
            service.StartupType = "Desconhecido";
        }
    }
}
