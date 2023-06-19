using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScreenShare.Recorder
{
    public class ScreenRecorder : IScreenRecorder
    {
        private Rectangle _bounds;

        public ScreenRecorder() 
        {
            _bounds = Screen.PrimaryScreen!.Bounds;
        }

        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);

        public ImageSource GetFrame()
        {
            //System.Drawing.Size sz = Screen.PrimaryScreen!.Bounds.Size;
            IntPtr hDesk = GetDesktopWindow();
            IntPtr hSrce = GetWindowDC(hDesk);
            IntPtr hDest = CreateCompatibleDC(hSrce);
            IntPtr hBmp = CreateCompatibleBitmap(hSrce, _bounds.Width, _bounds.Height);
            IntPtr hOldBmp = SelectObject(hDest, hBmp);
            BitBlt(
                hDest, 
                0, 
                0, 
                _bounds.Width, 
                _bounds.Height, 
                hSrce, 
                0, 
                0, 
                CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            SelectObject(hDest, hOldBmp);
            BitmapSource source;
            try
            {
                source = Imaging.CreateBitmapSourceFromHBitmap(
                                    hBmp,
                                    IntPtr.Zero,
                                    Int32Rect.Empty,
                                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBmp);
                DeleteDC(hDest);
                ReleaseDC(hDesk, hSrce);
                ReleaseDC(IntPtr.Zero, hSrce);
            }
            
            return source;
            //using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            //{
            //    using (Graphics g = Graphics.FromImage(bitmap))
            //    {
            //        g.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds.Size);
            //    }

            //    IntPtr handle = bitmap.GetHbitmap();
            //    BitmapSource source;
            //    try
            //    {
            //        source = Imaging.CreateBitmapSourceFromHBitmap(
            //                            handle,
            //                            IntPtr.Zero,
            //                            Int32Rect.Empty,
            //                            BitmapSizeOptions.FromEmptyOptions());
            //    }
            //    finally
            //    {
            //        DeleteObject(handle);
            //    }

            //    return source;
            //}
        }
    }
}
