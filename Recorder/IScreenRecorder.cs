using ScreenShare.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ScreenShare.Recorder
{
    public interface IScreenRecorder
    {
        public ImageSource GetFrame();
    }
}
