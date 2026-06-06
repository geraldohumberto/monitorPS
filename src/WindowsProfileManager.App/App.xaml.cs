using System.Windows;
using System.Windows.Threading;
using WindowsProfileManager.App.Services;

namespace WindowsProfileManager.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        base.OnStartup(e);
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        AppLogger.Write("Erro nao tratado na interface.", e.Exception);
        MessageBox.Show(
            $"Ocorreu um erro, mas o aplicativo continuara aberto.\n\nLog: {AppLogger.LogFilePath}\n\n{e.Exception.Message}",
            "Perfil Windows",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        AppLogger.Write("Erro fatal nao tratado.", e.ExceptionObject as Exception);
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        AppLogger.Write("Erro nao observado em tarefa.", e.Exception);
        e.SetObserved();
    }
}
