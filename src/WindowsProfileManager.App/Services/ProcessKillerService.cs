using System.Diagnostics;
using WindowsProfileManager.App.Models;

namespace WindowsProfileManager.App.Services;

public sealed class ProcessKillerService
{
    public string Kill(ProcessItem item)
    {
        if (item.IsProtected)
        {
            return $"Bloqueado: {item.Name} e protegido.";
        }

        try
        {
            using var process = Process.GetProcessById(item.Pid);
            process.Kill();
            return $"OK: processo encerrado {item.Name} ({item.Pid}).";
        }
        catch (Exception ex)
        {
            return $"Falha ao encerrar {item.Name} ({item.Pid}): {ex.Message}";
        }
    }
}
