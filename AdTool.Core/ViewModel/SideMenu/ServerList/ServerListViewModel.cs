using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdTool.Core
{
    public class ServerListViewModel : BaseViewModel
    {
        public ObservableCollection<ServerListItemViewModel> Items { get; set; }
    }
}
