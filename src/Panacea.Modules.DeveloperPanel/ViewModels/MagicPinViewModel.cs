using AForge.Video.DirectShow;
using Panacea.Controls;
using Panacea.Core;
using Panacea.Modules.DeveloperPanel.Views;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ZXing;

namespace Panacea.Modules.DeveloperPanel.ViewModels
{
    [View(typeof(MagicPinPage))]
    class MagicPinViewModel : ViewModelBase
    {
        BarcodeReader _reader = new BarcodeReader();
        VideoCaptureDevice _capture;


        public MagicPinViewModel(DeveloperPanelPlugin plugin, PanaceaServices core)
        {
            _core = core;

            MachineName = Environment.MachineName;
            ShowDevPageCommand = new RelayCommand(args =>
            {
                plugin.ShowDevPage();
            });
            StartLauncherCommand = new RelayCommand(args =>
            {
                try
                {
                    var path = Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Updater",
                        "Support",
                        "Launcher",
                        "PanaceaLauncher");

                    Process.Start(path, "/show");
                }
                catch (Exception ex)
                {

                }
            });
            UnlockCommand = new RelayCommand(args =>
            {
                try
                {
                    var now = DateTime.Now;
                    var first = now.Date.Month + now.Hour;
                    var second = now.Date.Day + now.Minute;
                    var pin = first * 100 + second;
                    var input = Int32.Parse(Pin);
                    if (input >= pin - 1 && input <= pin + 1)
                    {
                        Unlocked = true;
                        Pin = "";
                        return;
                    }
                    Pin = "";
                    Error = "No!";
                }
                catch
                {
                    Error = "Oops! Something went wrong. Shame on us...";
                }
            },
            args => Pin?.Length >= 4);
        }


        public override void Activate()
        {
            base.Activate();
            var devs = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (devs.Count > 0)
            {
                _capture = new VideoCaptureDevice(devs[0].MonikerString);
                _capture.NewFrame += _capture_NewFrame;

            }
            if (_capture != null)
            {
                _capture.Start();

            }

        }

        public override void Deactivate()
        {
            base.Deactivate();
            if (_capture != null)
            {
                _capture.NewFrame -= _capture_NewFrame;
                _capture?.SignalToStop();
                _capture = null;
            }
        }

        BitmapImage _frame;
        public BitmapImage Frame
        {
            get => _frame;
            set
            {
                _frame = value;
                OnPropertyChanged();
            }
        }
        private async void _capture_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            _capture.NewFrame -= _capture_NewFrame;
            using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();
                    Frame = bitmapimage;
                });


                Result result = _reader.Decode(bitmap);
                if (result != null)
                {
                    try
                    {
                        string decoded = result.ToString().Trim();
                        if (decoded != "")
                        {

                            var req = (HttpWebRequest)WebRequest.Create(_core.HttpClient.RelativeToAbsoluteUri("admin/login"));

                            var postData = "email=" + Uri.EscapeDataString(decoded);
                            postData += "&password=" + Uri.EscapeDataString("doesnotexist");
                            var data = Encoding.ASCII.GetBytes(postData);

                            req.Method = "POST";
                            req.ContentType = "application/x-www-form-urlencoded";
                            req.ContentLength = data.Length;

                            using (var stream = req.GetRequestStream())
                            {
                                await stream.WriteAsync(data, 0, data.Length);
                            }

                            using (var response = (HttpWebResponse)(await req.GetResponseAsync()))
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {

                                var res = await reader.ReadToEndAsync();

                                if (res.Contains("Incorrect password!"))
                                {
                                    Unlocked = true;
                                    if (_capture != null)
                                    {
                                        _capture.NewFrame -= _capture_NewFrame;
                                        _capture?.SignalToStop();

                                        _capture = null;
                                    }
                                    Frame = null;
                                    return;
                                }
                            }
                            // _timer.Stop();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }
            _capture.NewFrame += _capture_NewFrame;

        }

        string _pin;
        public string Pin
        {
            get => _pin;
            set
            {
                _pin = value;
                OnPropertyChanged();
            }
        }

        string _error;
        public string Error
        {
            get => _error;
            set
            {
                _error = value;
                OnPropertyChanged();
            }
        }

        public string MachineName { get; set; }

        public ICommand UnlockCommand { get; }

        public ICommand ShowDevPageCommand { get; }

        public ICommand StartLauncherCommand { get; }

        bool _unlocked;
        private readonly PanaceaServices _core;

        public bool Unlocked
        {
            get => _unlocked;
            set
            {
                _unlocked = value;
                OnPropertyChanged();
            }
        }

        public static System.Drawing.Bitmap BitmapSourceToBitmap2(BitmapSource srs)
        {
            int width = srs.PixelWidth;
            int height = srs.PixelHeight;
            int stride = width * ((srs.Format.BitsPerPixel + 7) / 8);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(height * stride);
                srs.CopyPixels(new Int32Rect(0, 0, width, height), ptr, height * stride, stride);
                using (var btm = new System.Drawing.Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format1bppIndexed, ptr))
                {
                    // Clone the bitmap so that we can dispose it and
                    // release the unmanaged memory at ptr
                    return new System.Drawing.Bitmap(btm);
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
