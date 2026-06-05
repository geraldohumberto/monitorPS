using System.Collections.ObjectModel;

namespace WindowsProfileManager.App.Models;

public enum ComparisonStatus
{
    Unknown,
    InProfile,
    Extra,
    Missing,
    Protected
}

public enum PendingActionType
{
    KillProcess,
    StartService,
    StopService,
    RestartService,
    SetServiceAutomatic,
    SetServiceManual,
    DisableService,
    DisableStartupItem,
    AddAllowed
}

public sealed class WindowsProfile
{
    public string ProfileName { get; set; } = "Perfil Windows";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string MachineName { get; set; } = Environment.MachineName;
    public string OsVersion { get; set; } = Environment.OSVersion.VersionString;
    public List<ProcessItem> Processes { get; set; } = [];
    public List<ServiceItem> Services { get; set; } = [];
    public List<StartupItem> StartupItems { get; set; } = [];
}

public sealed class ProcessItem : SelectableItem
{
    public string Name { get; set; } = "";
    public int Pid { get; set; }
    public double CpuSeconds { get; set; }
    public long RamBytes { get; set; }
    public string Path { get; set; } = "";
    public string User { get; set; } = "";
    public string Category { get; set; } = "Aplicativo";
    public bool Allowed { get; set; }
    public bool IsProtected { get; set; }
    public ComparisonStatus ComparisonStatus { get; set; } = ComparisonStatus.Unknown;
    public string RamDisplay => $"{RamBytes / 1024d / 1024d:N1} MB";
}

public sealed class ServiceItem : SelectableItem
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Status { get; set; } = "";
    public string StartupType { get; set; } = "";
    public string ExecutablePath { get; set; } = "";
    public string Category { get; set; } = "Servico";
    public bool Allowed { get; set; }
    public bool IsProtected { get; set; }
    public ComparisonStatus ComparisonStatus { get; set; } = ComparisonStatus.Unknown;
}

public sealed class StartupItem : SelectableItem
{
    public string Name { get; set; } = "";
    public string Source { get; set; } = "";
    public bool Enabled { get; set; }
    public string Command { get; set; } = "";
    public string Category { get; set; } = "Inicializacao";
    public bool Allowed { get; set; }
    public bool IsProtected { get; set; }
    public ComparisonStatus ComparisonStatus { get; set; } = ComparisonStatus.Unknown;
}

public sealed class PendingAction : SelectableItem
{
    public PendingActionType Type { get; set; }
    public string TargetKind { get; set; } = "";
    public string TargetName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "Pendente";
}

public abstract class SelectableItem : ObservableObject
{
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public sealed class DashboardSummary : ObservableObject
{
    private int _processCount;
    private int _runningServicesCount;
    private int _startupCount;
    private int _extrasCount;
    private int _protectedCount;
    private int _pendingCount;

    public int ProcessCount { get => _processCount; set => SetProperty(ref _processCount, value); }
    public int RunningServicesCount { get => _runningServicesCount; set => SetProperty(ref _runningServicesCount, value); }
    public int StartupCount { get => _startupCount; set => SetProperty(ref _startupCount, value); }
    public int ExtrasCount { get => _extrasCount; set => SetProperty(ref _extrasCount, value); }
    public int ProtectedCount { get => _protectedCount; set => SetProperty(ref _protectedCount, value); }
    public int PendingCount { get => _pendingCount; set => SetProperty(ref _pendingCount, value); }
}

public sealed class ComparisonResult
{
    public ObservableCollection<ProcessItem> AllowedProcesses { get; } = [];
    public ObservableCollection<ProcessItem> ExtraProcesses { get; } = [];
    public ObservableCollection<ProcessItem> ProtectedProcesses { get; } = [];
}
