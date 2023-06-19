using ScreenShare.Recorder;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScreenShare.ViewModel;

public class MainPageViewModel : INotifyPropertyChanged
{
    private ImageSource? _screen;
    private IScreenRecorder _screenRecorder;

    public DelegateCommand StartShareCommand { get; private set; }
    public DelegateCommand StartVoiceCommand { get; private set; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public MainPageViewModel(IScreenRecorder screenRecorder)
    {
        _screenRecorder = screenRecorder;

        StartShareCommand = new DelegateCommand(StartScreenShare, (x) => true);
        StartVoiceCommand = new DelegateCommand(StartVoice, (x) => true);
    }

    private void StartScreenShare()
    {
        int fps = 30;
        System.Timers.Timer ScreenShare = new System.Timers.Timer(1000/fps);
        ScreenShare.Elapsed += UpdateScreen;
        ScreenShare.Start();
    }

    private void StartVoice()
    {

    }


    private void UpdateScreen(object? sender, ElapsedEventArgs e)
    {
        ImageSource frame = _screenRecorder.GetFrame();

        if (frame.CanFreeze)
        {
            frame.Freeze();
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            Screen = frame;
        });
    }

    public ImageSource? Screen
    {
        get => _screen;
        set
        {
            if (value == _screen) return;

            _screen = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
