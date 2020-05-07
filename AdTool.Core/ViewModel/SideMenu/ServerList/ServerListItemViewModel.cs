using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AdTool.Core
{
    public class ServerListItemViewModel : BaseViewModel
    {
        public ICommand OpenServerCommand { get; set; }
        
        public ServerListItemViewModel()
        {
            OpenServerCommand = new RelayCommand(OpenServer);
        }

        private void OpenServer()
        {
            mServerListDesignModel = ServerListDesignModel.Instance;

            foreach (var item in mServerListDesignModel.Items)
            {
                if (item.Name.Equals(Name))
                {
                    item.IsSelected = true;
                    item.NewContentAvailable = true;
                }
                else
                {
                    item.IsSelected = false;
                    item.NewContentAvailable = false;
                }
            }

            if (Name.Equals("CreateServer", StringComparison.OrdinalIgnoreCase))
                IoC.Get<ApplicationViewModel>().GoToPage(ApplicationPage.CreateServer);
            if (Name.Equals("CreateIp", StringComparison.OrdinalIgnoreCase))
                IoC.Get<ApplicationViewModel>().GoToPage(ApplicationPage.CreateIp);
            if (Name.Equals("SetAgentKey", StringComparison.OrdinalIgnoreCase))
                IoC.Get<ApplicationViewModel>().GoToPage(ApplicationPage.SetAgentKey);
            if (Name.Equals("SetAdGroup", StringComparison.OrdinalIgnoreCase))
                IoC.Get<ApplicationViewModel>().GoToPage(ApplicationPage.SetAdGroup);
            if (Name.Equals("SetAdPrimary", StringComparison.OrdinalIgnoreCase))
                IoC.Get<ApplicationViewModel>().GoToPage(ApplicationPage.SetAdPrimary);
            if (Name.Equals("SetAdSecondary", StringComparison.OrdinalIgnoreCase))
                IoC.Get<ApplicationViewModel>().GoToPage(ApplicationPage.SetAdSecondary);
        }

        ServerListDesignModel mServerListDesignModel;
        public string Name { get; set; }
        public string Message { get; set; }
        public string Number { get; set; }
        public string ProfilePictureRGB { get; set; }
        public bool NewContentAvailable { get; set; }
        public bool IsSelected { get; set; }
    }
}
