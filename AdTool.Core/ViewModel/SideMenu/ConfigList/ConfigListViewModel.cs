using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdTool.Core
{
    public class ConfigListViewModel : BaseViewModel, INotifyPropertyChanged
    {
        public ObservableCollection<ConfigListItemViewModel> Items { get; set; }
    }
}
