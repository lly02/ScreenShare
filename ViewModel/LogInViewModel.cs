using ScreenShare.Network.P2P;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenShare.ViewModel;

public class LogInViewModel
{
    private Server _server;

    public LogInViewModel()
    {
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        _server.StartServer();
    }
}
