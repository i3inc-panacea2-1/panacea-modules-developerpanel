using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.UiManager;
using Panacea.Modules.DeveloperPanel.Models;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Panacea.Modules.DeveloperPanel
{
    [View(typeof(DeveloperPanelControl))]
    class DeveloperPanelControlViewModel : ViewModelBase
    {

        private readonly ILogger _logger;
        private readonly PanaceaServices _core;
        ObservableCollection<Log> _logs;
        public ObservableCollection<Log> Logs
        {
            get => _logs;
            set
            {
                _logs = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<KeyValuePair<string, string>> Arguments { get; set; } = new ObservableCollection<KeyValuePair<string, string>>();

        ObservableCollection<PluginInfo> _plugins;
        public ObservableCollection<PluginInfo> Plugins
        {
            get => _plugins;
            set
            {
                _plugins = value;
                OnPropertyChanged();
            }
        }

        public DeveloperPanelControlViewModel(PanaceaServices core)
        {
            _logger = core.Logger;
            _core = core;
            Arguments.Add(new KeyValuePair<string, string>("Server", _core.HttpClient.RelativeToAbsoluteUri("")));
            Arguments.Add(new KeyValuePair<string, string>("PUTIK", _core.HttpClient.GetApiEndpoint("")));
            Arguments.Add(new KeyValuePair<string, string>("Arguments", string.Join("\r\n", Environment.GetCommandLineArgs())));
            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                var sb = new StringBuilder();
                IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                {
                    sb.AppendLine(addr.Address.ToString());
                }
                Arguments.Add(new KeyValuePair<string, string>("Network: " + netInterface.Name, sb.ToString()));


                Console.WriteLine("");
            }
            SetupCommands();
        }

        private void SetupCommands()
        {
            ExitCommand = new RelayCommand(args =>
            {
                ShutDownSafe();
            });
            RestartCommand = new RelayCommand(async args =>
            {
                await _core.UserService.LogoutAsync();
                ShutDownSafe();
            });
            RestartWithUserCommand = new RelayCommand(args =>
            {
                ShutDownSafe();
            });
            OpenInWindowCommand = new RelayCommand(args =>
            {
                if (_core.TryGetUiManager(out IUiManager ui))
                {
                    ui.GoHome();
                }
                var w = new Window()
                {
                    Content = new DeveloperPanelControlViewModel(_core).View
                };
                w.Show();

            });

            ExecuteCodeCommand = new RelayCommand(async args =>
            {
                try
                {
                    var ass = CodeHelper.CompileSourceCodeDom(Code);
                    await CodeHelper.ExecuteFromAssembly(ass, _core);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }
        string _code;
        public string Code
        {
            get => _code;
            set
            {
                _code = value;
                OnPropertyChanged();
            }
        }

        private void ShutDownSafe()
        {
            if (Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => Application.Current.Shutdown()));
            }
            else Application.Current.Shutdown();
        }

        public RelayCommand ExitCommand { get; set; }
        public RelayCommand RestartCommand { get; set; }
        public RelayCommand RestartWithUserCommand { get; set; }
        public RelayCommand OpenInWindowCommand { get; set; }

        public RelayCommand ExecuteCodeCommand { get; set; }

        public override void Activate()
        {
            HookLogs();
            LoadPlugins();
        }

        public override void Deactivate()
        {
            _logger.OnLog -= _logger_OnLog;
        }

        void LoadPlugins()
        {
            Plugins = new ObservableCollection<PluginInfo>();
            foreach (var p in _core.PluginLoader.LoadedPlugins)
            {
                Plugins.Add(new PluginInfo()
                {
                    Name = p.Key,
                    ClassName = p.Value.GetType().FullName,
                    FileName = p.Value.GetType().Assembly.Location,
                    Version = FileVersionInfo.GetVersionInfo(p.Value.GetType().Assembly.Location).FileVersion.ToString()
                });
            }
        }

        void HookLogs()
        {
            _logger.OnLog += _logger_OnLog;
            Logs = new ObservableCollection<Log>();
            foreach (var log in _logger.Logs.ToList())
            {
                Logs.Insert(0, log);
            }
        }
        string _searchTerm = "";
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                Logs = new ObservableCollection<Log>();
                var term = _searchTerm.ToLower();
                foreach (var log in _logger.Logs.ToList())
                {
                    if (log.Message.ToLower().Contains(term)
                        || log.Sender.ToLower().Contains(term))
                    {
                        Logs.Insert(0, log);
                    }
                }
            }
        }
        private void _logger_OnLog(object sender, Log e)
        {
            if (Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var term = _searchTerm.ToLower();
                    if (e.Message.ToLower().Contains(term)
                           || e.Sender.ToLower().Contains(term))
                    {
                        Logs.Insert(0, e);
                    }
                }), DispatcherPriority.Background);
            }
        }
    }


    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string EnumString;
            try
            {
                EnumString = Enum.GetName((value.GetType()), value);
                return EnumString;
            }
            catch
            {
                return string.Empty;
            }
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
