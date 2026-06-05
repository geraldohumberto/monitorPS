using WindowsProfileManager.App.Models;

namespace WindowsProfileManager.App.Services;

public sealed class ProfileComparer
{
    public void Compare(WindowsProfile current, WindowsProfile loaded)
    {
        var allowedProcesses = loaded.Processes
            .Select(p => NormalizeProcessName(p.Name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var services = loaded.Services
            .Select(s => s.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var startupItems = loaded.StartupItems
            .Select(s => NormalizeStartupKey(s.Name, s.Source))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var process in current.Processes)
        {
            process.ComparisonStatus = process.IsProtected
                ? ComparisonStatus.Protected
                : allowedProcesses.Contains(NormalizeProcessName(process.Name))
                    ? ComparisonStatus.InProfile
                    : ComparisonStatus.Extra;
        }

        foreach (var service in current.Services)
        {
            service.ComparisonStatus = service.IsProtected
                ? ComparisonStatus.Protected
                : services.Contains(service.Name) ? ComparisonStatus.InProfile : ComparisonStatus.Extra;
        }

        foreach (var item in current.StartupItems)
        {
            item.ComparisonStatus = item.IsProtected
                ? ComparisonStatus.Protected
                : startupItems.Contains(NormalizeStartupKey(item.Name, item.Source))
                    ? ComparisonStatus.InProfile
                    : ComparisonStatus.Extra;
        }
    }

    private static string NormalizeProcessName(string name)
    {
        return name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? name : $"{name}.exe";
    }

    private static string NormalizeStartupKey(string name, string source) => $"{source}|{name}";
}
