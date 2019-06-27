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
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                ui.PreviewKeyDown += Ui_PreviewKeyDown;
                _button = new NavigationButtonViewModel(_core);
                ui.AddNavigationBarControl(_button);
            }
            return Task.CompletedTask;
        }

        private void Ui_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
