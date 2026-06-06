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
            var fieldName = GetFieldName(line);
            if (IsServiceNameField(fieldName))
            {
                current = new ServiceItem { Name = GetFieldValue(line) };
                result.Add(current);
                continue;
            }

            if (current is null)
            {
                continue;
            }

            if (IsDisplayNameField(fieldName))
            {
                current.DisplayName = GetFieldValue(line);
            }
            else if (IsStateField(fieldName))
            {
                current.Status = GetFieldValue(line);
            }
        }

        foreach (var service in result.Where(s => string.IsNullOrWhiteSpace(s.DisplayName)))
        {
            service.DisplayName = service.Name;
        }

        return result;
    }

    private static string GetFieldName(string line)
    {
        var index = line.IndexOf(':');
        return index < 0 ? line : line[..index].Trim();
    }

    private static string GetFieldValue(string line)
    {
        var index = line.IndexOf(':');
        return index < 0 ? "" : line[(index + 1)..].Trim();
    }

    private static string NormalizeFieldName(string fieldName)
    {
        return fieldName
            .Replace("_", " ", StringComparison.Ordinal)
            .Replace("�", "", StringComparison.Ordinal)
            .ToUpperInvariant();
    }

    private static bool IsServiceNameField(string fieldName)
    {
        var normalized = NormalizeFieldName(fieldName);
        return normalized.Equals("SERVICE NAME", StringComparison.Ordinal)
            || (normalized.Contains("NOME", StringComparison.Ordinal)
                && normalized.Contains("SERVI", StringComparison.Ordinal)
                && !normalized.Contains("EXIB", StringComparison.Ordinal));
    }

    private static bool IsDisplayNameField(string fieldName)
    {
        var normalized = NormalizeFieldName(fieldName);
        return normalized.Equals("DISPLAY NAME", StringComparison.Ordinal)
            || (normalized.Contains("NOME", StringComparison.Ordinal)
                && normalized.Contains("EXIB", StringComparison.Ordinal));
    }

    private static bool IsStateField(string fieldName)
    {
        var normalized = NormalizeFieldName(fieldName);
        return normalized.Equals("STATE", StringComparison.Ordinal)
            || normalized.Equals("ESTADO", StringComparison.Ordinal);
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
