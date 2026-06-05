using System.Text;
using WindowsProfileManager.App.Models;

namespace WindowsProfileManager.App.Services;

public sealed class ReportWriter
{
    public string CreateReport(WindowsProfile profile, IEnumerable<string> logLines)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Perfil Windows - Relatorio");
        builder.AppendLine($"Gerado em: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"Maquina: {profile.MachineName}");
        builder.AppendLine($"Sistema: {profile.OsVersion}");
        builder.AppendLine();
        builder.AppendLine($"Processos: {profile.Processes.Count}");
        builder.AppendLine($"Servicos: {profile.Services.Count}");
        builder.AppendLine($"Inicializacao: {profile.StartupItems.Count}");
        builder.AppendLine();
        builder.AppendLine("Eventos:");

        foreach (var line in logLines)
        {
            builder.AppendLine($"- {line}");
        }

        return builder.ToString();
    }

    public void SaveText(string filePath, WindowsProfile profile, IEnumerable<string> logLines)
    {
        File.WriteAllText(filePath, CreateReport(profile, logLines));
    }
}
