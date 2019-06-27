using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.UiManager;
using Panacea.Modules.DeveloperPanel.Views;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Panacea.Modules.DeveloperPanel.ViewModels
{
    [View(typeof(NavigationButton))]
    class NavigationButtonViewModel : ViewModelBase
    {
        public NavigationButtonViewModel(PanaceaServices core)
        {
            ClickCommand = new RelayCommand(args =>
            {
                if (core.TryGetUiManager(out IUiManager ui))
                {
                    ui.Navigate(new MagicPinViewModel(), false);
                }
            });
        }
        public ICommand ClickCommand { get; }
    }
}
