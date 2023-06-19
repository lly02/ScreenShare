using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenShare.Network.P2P;
using ScreenShare.Recorder;
using ScreenShare.View;
using ScreenShare.ViewModel;

namespace ScreenShare;

public partial class App : Application
{
    public IHost? AppHost { get; private set; }

    public App()
    {
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.AddTransient<Server>();
            services.AddTransient<Network.P2P.Payload.Decoder>();
            services.AddTransient<IScreenRecorder, ScreenRecorder>();

            services.AddTransient<LogIn>();
            services.AddTransient<LogInViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<MainPageViewModel>();
        });
        AppHost = builder.Build();

        var logInPage = AppHost!.Services.GetRequiredService<MainPage>();
        logInPage.InitializeComponent();
        logInPage.Show();
    }
}
