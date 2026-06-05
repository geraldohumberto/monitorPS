using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;
using WindowsProfileManager.App.Models;
using WindowsProfileManager.App.Services;

namespace WindowsProfileManager.App.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly ProcessScannerService _processScanner;
    private readonly WindowsServiceScannerService _serviceScanner;
    private readonly StartupScannerService _startupScanner;
    private readonly ProfileSerializer _serializer;
    private readonly ProfileComparer _comparer;
    private readonly ProcessKillerService _processKiller;
    private readonly ServiceControlService _serviceControl;
    private readonly StartupControlService _startupControl;
    private readonly AdminPermissionService _adminPermission;
    private readonly ReportWriter _reportWriter;
    private readonly ObservableCollection<string> _logLines = [];
    private WindowsProfile _currentProfile = new();
    private WindowsProfile? _loadedProfile;
    private string _loadedProfileName = "nenhum";
    private string _adminStatus = "";
    private string _reportText = "";

    public MainViewModel()
    {
        var protection = new ProtectionCatalog();
        var commandRunner = new CommandRunner();
        _processScanner = new ProcessScannerService(protection);
        _serviceScanner = new WindowsServiceScannerService(commandRunner, protection);
        _startupScanner = new StartupScannerService();
        _serializer = new ProfileSerializer();
        _comparer = new ProfileComparer();
        _processKiller = new ProcessKillerService();
        _serviceControl = new ServiceControlService(commandRunner);
        _startupControl = new StartupControlService();
        _adminPermission = new AdminPermissionService();
        _reportWriter = new ReportWriter();

        ScanCommand = new RelayCommand(Scan);
        SaveProfileCommand = new RelayCommand(SaveProfile, HasScan);
        ExportReportCommand = new RelayCommand(ExportReport, HasScan);
        LoadProfileCommand = new RelayCommand(LoadProfile);
        CompareCommand = new RelayCommand(Compare, () => HasScan() && _loadedProfile is not null);
        RestartAsAdminCommand = new RelayCommand(_adminPermission.RestartAsAdministrator);
        KillNowCommand = new RelayCommand(() => KillSelected(Processes.Where(p => p.IsSelected)));
        QueueKillCommand = new RelayCommand(() => QueueProcesses(Processes.Where(p => p.IsSelected)));
        AddProcessAllowedCommand = new RelayCommand(() => AddAllowed(Processes.Where(p => p.IsSelected).Select(p => p.Name), "Processo"));
        StartServiceCommand = new RelayCommand(() => RunServiceAction(Services.Where(s => s.IsSelected), _serviceControl.Start));
        StopServiceCommand = new RelayCommand(() => RunServiceAction(Services.Where(s => s.IsSelected), _serviceControl.Stop));
        RestartServiceCommand = new RelayCommand(() => RunServiceAction(Services.Where(s => s.IsSelected), _serviceControl.Restart));
        SetAutomaticCommand = new RelayCommand(() => RunServiceAction(Services.Where(s => s.IsSelected), s => _serviceControl.SetStartup(s, "auto")));
        SetManualCommand = new RelayCommand(() => RunServiceAction(Services.Where(s => s.IsSelected), s => _serviceControl.SetStartup(s, "demand")));
        DisableServiceCommand = new RelayCommand(() => RunServiceAction(Services.Where(s => s.IsSelected), s => _serviceControl.SetStartup(s, "disabled")));
        AddServiceAllowedCommand = new RelayCommand(() => AddAllowed(Services.Where(s => s.IsSelected).Select(s => s.Name), "Servico"));
        QueueDisableServiceCommand = new RelayCommand(() => QueueServices(Services.Where(s => s.IsSelected), PendingActionType.DisableService, "Desativar servico"));
        QueueManualServiceCommand = new RelayCommand(() => QueueServices(Services.Where(s => s.IsSelected), PendingActionType.SetServiceManual, "Mudar servico para manual"));
        DisableStartupCommand = new RelayCommand(() => RunStartupAction(StartupItems.Where(s => s.IsSelected)));
        AddStartupAllowedCommand = new RelayCommand(() => AddAllowed(StartupItems.Where(s => s.IsSelected).Select(s => s.Name), "Inicializacao"));
        QueueDisableStartupCommand = new RelayCommand(() => QueueStartup(StartupItems.Where(s => s.IsSelected)));
        RemovePendingCommand = new RelayCommand(RemoveSelectedPending);
        ClearPendingCommand = new RelayCommand(() =>
        {
            PendingActions.Clear();
            RefreshSummary();
        });
        ApplyPendingCommand = new RelayCommand(ApplyPending);
        SavePendingCommand = new RelayCommand(SavePending);
        DetailsCommand = new RelayCommand(ShowDetails);

        AdminStatus = _adminPermission.IsAdministrator()
            ? "Permissao atual: administrador"
            : "Permissao atual: usuario comum. Algumas acoes podem falhar.";
    }

    public ObservableCollection<ProcessItem> Processes { get; } = [];
    public ObservableCollection<ServiceItem> Services { get; } = [];
    public ObservableCollection<StartupItem> StartupItems { get; } = [];
    public ObservableCollection<PendingAction> PendingActions { get; } = [];
    public DashboardSummary Summary { get; } = new();
    public ComparisonResult Comparison { get; } = new();

    public string LoadedProfileName { get => _loadedProfileName; set => SetProperty(ref _loadedProfileName, value); }
    public string AdminStatus { get => _adminStatus; set => SetProperty(ref _adminStatus, value); }
    public string ReportText { get => _reportText; set => SetProperty(ref _reportText, value); }

    public RelayCommand ScanCommand { get; }
    public RelayCommand SaveProfileCommand { get; }
    public RelayCommand ExportReportCommand { get; }
    public RelayCommand LoadProfileCommand { get; }
    public RelayCommand CompareCommand { get; }
    public RelayCommand RestartAsAdminCommand { get; }
    public RelayCommand KillNowCommand { get; }
    public RelayCommand QueueKillCommand { get; }
    public RelayCommand AddProcessAllowedCommand { get; }
    public RelayCommand StartServiceCommand { get; }
    public RelayCommand StopServiceCommand { get; }
    public RelayCommand RestartServiceCommand { get; }
    public RelayCommand SetAutomaticCommand { get; }
    public RelayCommand SetManualCommand { get; }
    public RelayCommand DisableServiceCommand { get; }
    public RelayCommand AddServiceAllowedCommand { get; }
    public RelayCommand QueueDisableServiceCommand { get; }
    public RelayCommand QueueManualServiceCommand { get; }
    public RelayCommand DisableStartupCommand { get; }
    public RelayCommand AddStartupAllowedCommand { get; }
    public RelayCommand QueueDisableStartupCommand { get; }
    public RelayCommand RemovePendingCommand { get; }
    public RelayCommand ClearPendingCommand { get; }
    public RelayCommand ApplyPendingCommand { get; }
    public RelayCommand SavePendingCommand { get; }
    public RelayCommand DetailsCommand { get; }

    private bool HasScan() => Processes.Count > 0 || Services.Count > 0 || StartupItems.Count > 0;

    private void Scan()
    {
        Processes.ReplaceWith(_processScanner.Scan());
        Services.ReplaceWith(_serviceScanner.Scan());
        StartupItems.ReplaceWith(_startupScanner.Scan(Services.ToList()));

        _currentProfile = new WindowsProfile
        {
            ProfileName = $"Perfil {Environment.MachineName}",
            CreatedAt = DateTime.Now,
            MachineName = Environment.MachineName,
            OsVersion = Environment.OSVersion.VersionString,
            Processes = Processes.ToList(),
            Services = Services.ToList(),
            StartupItems = StartupItems.ToList()
        };

        Log($"Varredura concluida: {Processes.Count} processos, {Services.Count} servicos, {StartupItems.Count} inicializacao.");
        if (_loadedProfile is not null)
        {
            Compare();
        }

        RefreshSummary();
    }

    private void SaveProfile()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Perfil JSON (*.json)|*.json",
            FileName = $"perfil-{Environment.MachineName}-{DateTime.Now:yyyyMMdd-HHmm}.json"
        };

        if (dialog.ShowDialog() == true)
        {
            RefreshCurrentProfile();
            _serializer.Save(dialog.FileName, _currentProfile);
            Log($"Perfil salvo em {dialog.FileName}.");
        }
    }

    private void ExportReport()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Relatorio TXT (*.txt)|*.txt",
            FileName = $"relatorio-{DateTime.Now:yyyyMMdd-HHmm}.txt"
        };

        if (dialog.ShowDialog() == true)
        {
            RefreshCurrentProfile();
            _reportWriter.SaveText(dialog.FileName, _currentProfile, _logLines);
            Log($"Relatorio exportado em {dialog.FileName}.");
        }
    }

    private void LoadProfile()
    {
        var dialog = new OpenFileDialog { Filter = "Perfil JSON (*.json)|*.json" };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        _loadedProfile = _serializer.Load(dialog.FileName);
        LoadedProfileName = System.IO.Path.GetFileName(dialog.FileName);
        Log($"Perfil carregado: {LoadedProfileName}.");

        if (HasScan())
        {
            Compare();
        }
    }

    private void Compare()
    {
        if (_loadedProfile is null)
        {
            return;
        }

        RefreshCurrentProfile();
        _comparer.Compare(_currentProfile, _loadedProfile);
        RefreshComparison();
        RefreshSummary();
        Log("Comparacao concluida.");
    }

    private void KillSelected(IEnumerable<ProcessItem> items)
    {
        foreach (var item in items.ToList())
        {
            Log(_processKiller.Kill(item));
        }

        Scan();
    }

    private void QueueProcesses(IEnumerable<ProcessItem> items)
    {
        foreach (var item in items.ToList())
        {
            AddPending(PendingActionType.KillProcess, "Processo", item.Name, $"Encerrar processo {item.Name} ({item.Pid})");
        }
    }

    private void RunServiceAction(IEnumerable<ServiceItem> items, Func<ServiceItem, string> action)
    {
        foreach (var item in items.ToList())
        {
            Log(action(item));
        }

        Scan();
    }

    private void QueueServices(IEnumerable<ServiceItem> items, PendingActionType type, string label)
    {
        foreach (var item in items.ToList())
        {
            AddPending(type, "Servico", item.Name, $"{label}: {item.DisplayName}");
        }
    }

    private void RunStartupAction(IEnumerable<StartupItem> items)
    {
        foreach (var item in items.ToList())
        {
            Log(_startupControl.Disable(item));
        }

        Scan();
    }

    private void QueueStartup(IEnumerable<StartupItem> items)
    {
        foreach (var item in items.ToList())
        {
            AddPending(PendingActionType.DisableStartupItem, "Inicializacao", item.Name, $"Desativar inicializacao {item.Name}");
        }
    }

    private void ApplyPending()
    {
        var message = "Aplicar as acoes pendentes agora? Essa operacao pode encerrar processos e alterar servicos.";
        if (MessageBox.Show(message, "Confirmar acoes", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        foreach (var action in PendingActions.ToList())
        {
            action.Status = ExecutePending(action);
            Log(action.Status);
        }

        RefreshSummary();
        Scan();
    }

    private string ExecutePending(PendingAction action)
    {
        return action.Type switch
        {
            PendingActionType.KillProcess => ExecuteProcessPending(action),
            PendingActionType.DisableService => ExecuteServicePending(action, s => _serviceControl.SetStartup(s, "disabled")),
            PendingActionType.SetServiceManual => ExecuteServicePending(action, s => _serviceControl.SetStartup(s, "demand")),
            PendingActionType.DisableStartupItem => ExecuteStartupPending(action),
            PendingActionType.AddAllowed => ExecuteAddAllowed(action),
            _ => $"Acao registrada: {action.Description}"
        };
    }

    private string ExecuteProcessPending(PendingAction action)
    {
        var process = Processes.FirstOrDefault(p => p.Name.Equals(action.TargetName, StringComparison.OrdinalIgnoreCase));
        return process is null ? $"Ignorado: processo nao encontrado {action.TargetName}." : _processKiller.Kill(process);
    }

    private string ExecuteServicePending(PendingAction action, Func<ServiceItem, string> execute)
    {
        var service = Services.FirstOrDefault(s => s.Name.Equals(action.TargetName, StringComparison.OrdinalIgnoreCase));
        return service is null ? $"Ignorado: servico nao encontrado {action.TargetName}." : execute(service);
    }

    private string ExecuteStartupPending(PendingAction action)
    {
        var item = StartupItems.FirstOrDefault(s => s.Name.Equals(action.TargetName, StringComparison.OrdinalIgnoreCase));
        return item is null ? $"Ignorado: inicializacao nao encontrada {action.TargetName}." : _startupControl.Disable(item);
    }

    private string ExecuteAddAllowed(PendingAction action)
    {
        foreach (var item in Processes.Where(p => action.TargetKind == "Processo" && p.Name.Equals(action.TargetName, StringComparison.OrdinalIgnoreCase)))
        {
            item.Allowed = true;
            item.ComparisonStatus = ComparisonStatus.InProfile;
        }

        foreach (var item in Services.Where(s => action.TargetKind == "Servico" && s.Name.Equals(action.TargetName, StringComparison.OrdinalIgnoreCase)))
        {
            item.Allowed = true;
            item.ComparisonStatus = ComparisonStatus.InProfile;
        }

        foreach (var item in StartupItems.Where(s => action.TargetKind == "Inicializacao" && s.Name.Equals(action.TargetName, StringComparison.OrdinalIgnoreCase)))
        {
            item.Allowed = true;
            item.ComparisonStatus = ComparisonStatus.InProfile;
        }

        RefreshCurrentProfile();
        RefreshComparison();
        return $"OK: item marcado como permitido: {action.TargetKind} {action.TargetName}.";
    }

    private void AddAllowed(IEnumerable<string> names, string kind)
    {
        foreach (var name in names.ToList())
        {
            AddPending(PendingActionType.AddAllowed, kind, name, $"Adicionar aos permitidos: {kind} {name}");
        }
    }

    private void AddPending(PendingActionType type, string kind, string name, string description)
    {
        PendingActions.Add(new PendingAction
        {
            Type = type,
            TargetKind = kind,
            TargetName = name,
            Description = description
        });
        RefreshSummary();
    }

    private void RemoveSelectedPending()
    {
        foreach (var item in PendingActions.Where(p => p.IsSelected).ToList())
        {
            PendingActions.Remove(item);
        }

        RefreshSummary();
    }

    private void SavePending()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Acoes JSON (*.json)|*.json",
            FileName = $"acoes-pendentes-{DateTime.Now:yyyyMMdd-HHmm}.json"
        };

        if (dialog.ShowDialog() == true)
        {
            File.WriteAllText(dialog.FileName, System.Text.Json.JsonSerializer.Serialize(PendingActions, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            Log($"Acoes pendentes salvas em {dialog.FileName}.");
        }
    }

    private void ShowDetails(object? parameter)
    {
        var text = parameter switch
        {
            ProcessItem p => $"{p.Name}\nPID: {p.Pid}\nRAM: {p.RamDisplay}\nCaminho: {p.Path}\nStatus: {p.ComparisonStatus}",
            ServiceItem s => $"{s.DisplayName}\nNome: {s.Name}\nStatus: {s.Status}\nInicializacao: {s.StartupType}\nCaminho: {s.ExecutablePath}",
            StartupItem i => $"{i.Name}\nFonte: {i.Source}\nAtivo: {i.Enabled}\nComando: {i.Command}",
            _ => "Selecione um item para ver detalhes."
        };

        MessageBox.Show(text, "Detalhes", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void RefreshCurrentProfile()
    {
        _currentProfile.Processes = Processes.ToList();
        _currentProfile.Services = Services.ToList();
        _currentProfile.StartupItems = StartupItems.ToList();
        _currentProfile.CreatedAt = DateTime.Now;
    }

    private void RefreshComparison()
    {
        Comparison.AllowedProcesses.ReplaceWith(Processes.Where(p => p.ComparisonStatus == ComparisonStatus.InProfile));
        Comparison.ExtraProcesses.ReplaceWith(Processes.Where(p => p.ComparisonStatus == ComparisonStatus.Extra));
        Comparison.ProtectedProcesses.ReplaceWith(Processes.Where(p => p.ComparisonStatus == ComparisonStatus.Protected));
    }

    private void RefreshSummary()
    {
        Summary.ProcessCount = Processes.Count;
        Summary.RunningServicesCount = Services.Count(s => s.Status.Contains("RUNNING", StringComparison.OrdinalIgnoreCase));
        Summary.StartupCount = StartupItems.Count;
        Summary.ExtrasCount = Processes.Count(p => p.ComparisonStatus == ComparisonStatus.Extra)
            + Services.Count(s => s.ComparisonStatus == ComparisonStatus.Extra)
            + StartupItems.Count(s => s.ComparisonStatus == ComparisonStatus.Extra);
        Summary.ProtectedCount = Processes.Count(p => p.IsProtected) + Services.Count(s => s.IsProtected) + StartupItems.Count(s => s.IsProtected);
        Summary.PendingCount = PendingActions.Count;
        ReportText = _reportWriter.CreateReport(_currentProfile, _logLines);
    }

    private void Log(string text)
    {
        _logLines.Insert(0, $"{DateTime.Now:HH:mm:ss} {text}");
        RefreshSummary();
    }
}

internal static class CollectionExtensions
{
    public static void ReplaceWith<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        collection.Clear();
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}
