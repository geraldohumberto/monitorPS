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

Console.WriteLine("WindowsProfileManager.Tests: OK");

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
