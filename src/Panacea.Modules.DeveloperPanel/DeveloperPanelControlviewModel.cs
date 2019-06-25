using Panacea.Core;
using Panacea.Modules.DeveloperPanel.Models;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        }

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
            foreach(var p in _core.PluginLoader.LoadedPlugins)
            {
                Plugins.Add(new PluginInfo()
                {
                    Name = p.Key,
                    ClassName = p.Value.GetType().FullName,
                    FileName = p.Value.GetType().Assembly.Location,
                    Version = ""
                });
            }
        }

        void HookLogs()
        {
            _logger.OnLog += _logger_OnLog;
            Logs = new ObservableCollection<Log>();
            foreach (var log in _logger.Logs.ToList())
            {
                Logs.Add(log);
            }
        }

        private void _logger_OnLog(object sender, Log e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Logs.Add(e);
            }), DispatcherPriority.Background);
            
        }
    }
}
