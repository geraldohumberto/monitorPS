using System.Windows;
using WindowsProfileManager.App.ViewModels;

namespace WindowsProfileManager.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
