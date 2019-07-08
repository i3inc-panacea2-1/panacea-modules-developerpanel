using Panacea.Controls;
using Panacea.Modules.DeveloperPanel.Views;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Panacea.Modules.DeveloperPanel.ViewModels
{
    [View(typeof(MagicPinPage))]
    class MagicPinViewModel : ViewModelBase
    {

        public MagicPinViewModel(DeveloperPanelPlugin plugin)
        {

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
        public bool Unlocked
        {
            get => _unlocked;
            set
            {
                _unlocked = value;
                OnPropertyChanged();
            }
        }
    }
}
