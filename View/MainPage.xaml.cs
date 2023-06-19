using ScreenShare.ViewModel;
using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ScreenShare.View;

public partial class MainPage : Window
{
    public MainPage(MainPageViewModel vm)
    {
        InitializeComponent();

        WindowState = WindowState.Maximized;
        DataContext = vm;
    }
}
