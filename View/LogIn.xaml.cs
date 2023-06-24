using ScreenShare.Network.P2P;
using ScreenShare.ViewModel;
using System.Windows;

namespace ScreenShare.View;

/// <summary>
/// Interaction logic for LogIn.xaml
/// </summary>
public partial class LogIn : Window
{
    private Server _server;

    public LogIn(LogInViewModel vm, Server server)
    {
        _server = server;
        InitializeComponent();

        DataContext = vm;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        _server.StartServer();
    }
}
