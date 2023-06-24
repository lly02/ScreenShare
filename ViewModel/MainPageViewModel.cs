using ScreenShare.Recorder.Interface;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ScreenShare.ViewModel;

public class MainPageViewModel : INotifyPropertyChanged
{
    private ImageSource? _screen;
    private string _screenShareButtonContent;
    private IScreenRecorder _screenRecorder;
    private bool _isRecording;
    private System.Timers.Timer _recorderLoop;
    private int _recordFps;

    private string _voiceButtonContent;
    private bool _isMuted;

    public DelegateCommand StartShareCommand { get; private set; }
    public DelegateCommand StartVoiceCommand { get; private set; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public MainPageViewModel(IScreenRecorder screenRecorder)
    {
        _screenRecorder = screenRecorder;

        StartShareCommand = new DelegateCommand(ScreenShareButtonClick, (x) => true);
        StartVoiceCommand = new DelegateCommand(VoiceButtonClick, (x) => true);

        _isRecording = false;
        _screenShareButtonContent = "Start Screen Share";
        _recordFps = 30;
        _recorderLoop = new System.Timers.Timer(1000 / _recordFps);
        _recorderLoop.Elapsed += UpdateScreen;

        _isMuted = true;
        _voiceButtonContent = "On Mic";
    }

    private void ScreenShareButtonClick()
    {
        if (_isRecording)
        {
            _isRecording = false;
            ScreenShareButtonContent = "Start Screen Share";
            StopScreenShare();
            return;
        }

        _isRecording = true;
        ScreenShareButtonContent = "Stop Screen Share";
        StartScreenShare();
    }

    private void StartScreenShare()
    {
        _recorderLoop.Start();
    }

    private void StopScreenShare()
    {
        _recorderLoop.Stop();
        ChangeScreen(null);
    }

    private void UpdateScreen(object? sender, ElapsedEventArgs e)
    {
        ImageSource frame = _screenRecorder.GetFrame();

        if (frame.CanFreeze)
        {
            frame.Freeze();
        }

        ChangeScreen(frame);
    }

    private void ChangeScreen(ImageSource? frame)
    {
        if (Application.Current == null)
        {
            return;
        }

        //if (_dispatcherOperation != null && !_isRecording)
        //{
        //    _dispatcherOperation
        //}

        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            if (!_isRecording)
            {
                Screen = null;
                return;
            }
            Screen = frame;
        });
    }

    private void VoiceButtonClick()
    {
        if (_isMuted)
        {
            _isMuted = false;
            VoiceButtonContent = "Off Mic";
            OnMic();
            return;
        }

        _isMuted = true;
        OffMic();
        VoiceButtonContent = "On Mic";
    }

    private void OnMic()
    {
    }

    private void OffMic()
    {

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

    public string ScreenShareButtonContent
    {
        get => _screenShareButtonContent;
        set
        {
            if (value == _screenShareButtonContent) return;
            _screenShareButtonContent = value;
            OnPropertyChanged();
        }
    }

    public string VoiceButtonContent
    {
        get => _voiceButtonContent;
        set
        {
            if (value == _voiceButtonContent) return;
            _voiceButtonContent = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
