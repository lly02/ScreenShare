using ScreenShare.Network.P2P;
using ScreenShare.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
