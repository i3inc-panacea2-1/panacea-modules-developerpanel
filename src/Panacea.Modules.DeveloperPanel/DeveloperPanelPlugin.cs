using Panacea.Core;
using Panacea.Modularity;
using Panacea.Modularity.UiManager;
using Panacea.Modules.DeveloperPanel.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Panacea.Modules.DeveloperPanel
{
    public class DeveloperPanelPlugin : IPlugin
    {
        private readonly PanaceaServices _core;
        NavigationButtonViewModel _button;
        public DeveloperPanelPlugin(PanaceaServices core)
        {
            _core = core;
        }

        public Task BeginInit()
        {

            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }

        public Task EndInit()
        {
            if (Debugger.IsAttached)
            {
                var w = new Window()
                {
                    Content = new DeveloperPanelControlViewModel(_core).View
                };
                w.Show();
            }
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                ui.PreviewKeyDown += Ui_PreviewKeyDown;
                _button = new NavigationButtonViewModel(_core, this);
                ui.AddNavigationBarControl(_button);
            }
            return Task.CompletedTask;
        }

        int _showDev = 0;

        internal void ShowDevPage()
        {
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                ui.Navigate(new DeveloperPanelControlViewModel(_core), false);
            }
        }

        private void Ui_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    ShowDevPage();
                }
            }
            else
            {
                if (new Key[] { Key.VolumeMute, Key.VolumeUp, Key.VolumeDown }.Contains(e.Key))
                {
                    e.Handled = true;
                    return;
                }
                if (Keyboard.IsKeyDown(Key.LeftCtrl)
                     && Keyboard.IsKeyDown(Key.Q))
                {

                    if (e.Key == Key.D7 && _showDev == 0)
                    {
                        _showDev = 1;
                        e.Handled = true;
                        return;
                    }
                    if (e.Key == Key.D8 && _showDev == 1)
                    {
                        _showDev = 2;
                        e.Handled = true;
                        return;
                    }
                    if (e.Key == Key.D9 && _showDev == 2)
                    {
                        e.Handled = true;
                        ShowDevPage();
                        _showDev = 0;
                        return;
                    }
                }
            }
        }

        public Task Shutdown()
        {
            if (_button != null && _core.TryGetUiManager(out IUiManager ui))
            {
                ui.RemoveNavigationBarControl(_button);
            }
            return Task.CompletedTask;
        }
    }
}
