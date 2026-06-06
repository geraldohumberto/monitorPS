using WindowsProfileManager.App.Models;
using WindowsProfileManager.App.Services;

var comparer = new ProfileComparer();
var current = new WindowsProfile
{
    Processes =
    [
        new ProcessItem { Name = "explorer.exe", IsProtected = true },
        new ProcessItem { Name = "discord.exe" }
    ],
    Services =
    [
        new ServiceItem { Name = "Spooler" }
    ],
    StartupItems =
    [
        new StartupItem { Name = "Steam", Source = "HKCU Run" }
    ]
};

var loaded = new WindowsProfile
{
    Processes = [new ProcessItem { Name = "explorer.exe" }],
    Services = [new ServiceItem { Name = "Spooler" }],
    StartupItems = [new StartupItem { Name = "Steam", Source = "HKCU Run" }]
};

comparer.Compare(current, loaded);

Assert(current.Processes[0].ComparisonStatus == ComparisonStatus.Protected, "protected process should stay protected");
Assert(current.Processes[1].ComparisonStatus == ComparisonStatus.Extra, "unknown process should be extra");
Assert(current.Services[0].ComparisonStatus == ComparisonStatus.InProfile, "known service should be in profile");
Assert(current.StartupItems[0].ComparisonStatus == ComparisonStatus.InProfile, "known startup item should be in profile");

var protection = new ProtectionCatalog();
var commandRunner = new CommandRunner();
var processScanner = new ProcessScannerService(protection);
var serviceScanner = new WindowsServiceScannerService(commandRunner, protection);
var startupScanner = new StartupScannerService();

var processes = processScanner.Scan();
Assert(processes.Count > 0, "process scanner should return real processes");
Assert(processes.All(p => !string.IsNullOrWhiteSpace(p.Name)), "all scanned processes should have names");

var services = serviceScanner.Scan();
Assert(services.Count > 0, "service scanner should return real services");
Assert(services.All(s => !string.IsNullOrWhiteSpace(s.Name)), "all scanned services should have names");

var startupItems = startupScanner.Scan(services);
Assert(startupItems.Count >= 0, "startup scanner should complete");

var realProfile = new WindowsProfile
{
    Processes = processes.ToList(),
    Services = services.ToList(),
    StartupItems = startupItems.ToList()
};

var report = new ReportWriter().CreateReport(realProfile, ["smoke test"]);
Assert(report.Contains("Processos:", StringComparison.OrdinalIgnoreCase), "report should include process count");

Console.WriteLine("WindowsProfileManager.Tests: OK");

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
