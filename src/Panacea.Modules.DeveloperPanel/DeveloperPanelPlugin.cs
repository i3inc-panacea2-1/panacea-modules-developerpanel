using Panacea.Core;
using Panacea.Modularity;
using Panacea.Modularity.UiManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Panacea.Modules.DeveloperPanel
{
    public class DeveloperPanelPlugin : IPlugin
    {
        private readonly PanaceaServices _core;

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
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                ui.PreviewKeyDown += Ui_PreviewKeyDown;
            }
            return Task.CompletedTask;
        }

        private void Ui_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    e.Handled = true;
                    if (_core.TryGetUiManager(out IUiManager ui))
                    {
                        ui.Navigate(new DeveloperPanelControlViewModel(_core), false);
                    }
                }
            }
            else
            {

            }
        }

        public Task Shutdown()
        {
            var control = new DeveloperPanelControlViewModel(_core);
            if (Debugger.IsAttached)
            {
                var window = new Window()
                {
                    Content = new DeveloperPanelControl()
                    {
                        DataContext = control
                    }
                };
                window.Show();
            }
            return Task.CompletedTask;
        }
    }
}
