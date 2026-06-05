using WindowsProfileManager.App.Models;

namespace WindowsProfileManager.App.Services;

public sealed class ServiceControlService
{
    private readonly CommandRunner _commandRunner;

    public ServiceControlService(CommandRunner commandRunner)
    {
        _commandRunner = commandRunner;
    }

    public string Start(ServiceItem item) => RunProtected(item, "start", "iniciado");
    public string Stop(ServiceItem item) => RunProtected(item, "stop", "parado");

    public string Restart(ServiceItem item)
    {
        if (item.IsProtected)
        {
            return $"Bloqueado: {item.Name} e protegido.";
        }

        var stop = _commandRunner.Run("sc.exe", $"stop \"{item.Name}\"");
        Thread.Sleep(700);
        var start = _commandRunner.Run("sc.exe", $"start \"{item.Name}\"");
        return $"{stop}{Environment.NewLine}{start}".Trim();
    }

    public string SetStartup(ServiceItem item, string startupType)
    {
        if (item.IsProtected && string.Equals(startupType, "disabled", StringComparison.OrdinalIgnoreCase))
        {
            return $"Bloqueado: {item.Name} e protegido.";
        }

        return _commandRunner.Run("sc.exe", $"config \"{item.Name}\" start= {startupType}");
    }

    private string RunProtected(ServiceItem item, string operation, string label)
    {
        if (item.IsProtected && operation.Equals("stop", StringComparison.OrdinalIgnoreCase))
        {
            return $"Bloqueado: {item.Name} e protegido.";
        }

        var result = _commandRunner.Run("sc.exe", $"{operation} \"{item.Name}\"");
        return string.IsNullOrWhiteSpace(result) ? $"OK: servico {label} {item.Name}." : result;
    }
}
