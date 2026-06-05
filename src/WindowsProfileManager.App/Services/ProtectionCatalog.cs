namespace WindowsProfileManager.App.Services;

public sealed class ProtectionCatalog
{
    private readonly HashSet<string> _protectedProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "System", "Registry", "Idle", "smss.exe", "csrss.exe", "wininit.exe", "winlogon.exe",
        "services.exe", "lsass.exe", "svchost.exe", "dwm.exe", "explorer.exe", "audiodg.exe",
        "fontdrvhost.exe", "nvcontainer.exe", "nvdisplay.container.exe", "nvidia share.exe",
        "nvidia web helper.exe", "nvcplui.exe", "radeonsoftware.exe", "amdow.exe",
        "amdrsserv.exe", "atiesrxx.exe", "atieclxx.exe", "cncmd.exe", "igfxem.exe",
        "igfxhk.exe", "igfxtray.exe", "IntelCpHDCPSvc.exe"
    };

    private readonly HashSet<string> _protectedServices = new(StringComparer.OrdinalIgnoreCase)
    {
        "EventLog", "PlugPlay", "RpcSs", "DcomLaunch", "Winmgmt", "LanmanWorkstation",
        "ProfSvc", "UserManager", "Schedule", "SamSs", "Power", "AudioSrv", "WlanSvc",
        "Dhcp", "Dnscache", "NlaSvc", "WinDefend", "SecurityHealthService"
    };

    public bool IsProtectedProcess(string name, int pid)
    {
        return pid <= 4 || _protectedProcesses.Contains(name);
    }

    public bool IsProtectedService(string name)
    {
        return _protectedServices.Contains(name);
    }
}
