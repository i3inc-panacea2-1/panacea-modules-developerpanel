using Panacea.Modules.DeveloperPanel.Views;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.DeveloperPanel.ViewModels
{
    [View(typeof(MagicPinPage))]
    class MagicPinViewModel : ViewModelBase
    {
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
