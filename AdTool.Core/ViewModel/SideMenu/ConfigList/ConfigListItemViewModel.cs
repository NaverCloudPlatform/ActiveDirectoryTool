using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AdTool.Core
{
    public class ConfigListItemViewModel : BaseViewModel
    {
        public ICommand OpenConfigCommand { get; set; }
                
        public ConfigListItemViewModel()
        {
            OpenConfigCommand = new RelayCommand(OpenCommand);
        }
        
        private void OpenCommand()
        {
            mConfigListDesignModel = ConfigListDesignModel.Instance;

            foreach (var item in mConfigListDesignModel.Items)
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

            if (Name.Equals("ObjectStorage", StringComparison.OrdinalIgnoreCase))
                IoC.Get<ApplicationViewModel>().GoToPage(ApplicationPage.ObjectStorage);
            if (Name.Equals("LoginKey", StringComparison.OrdinalIgnoreCase))
                IoC.Get<ApplicationViewModel>().GoToPage(ApplicationPage.LoginKey);
            if (Name.Equals("InitScript", StringComparison.OrdinalIgnoreCase))
                IoC.Get<ApplicationViewModel>().GoToPage(ApplicationPage.InitScript);
            if (Name.Equals("ConfigCheck", StringComparison.OrdinalIgnoreCase))
                IoC.Get<ApplicationViewModel>().GoToPage(ApplicationPage.ConfigCheck);
        }

        ConfigListDesignModel mConfigListDesignModel;
        public string Name { get; set; }
        public string Message { get; set; }
        public string Number { get; set; }
        public string ProfilePictureRGB { get; set; }
        public bool NewContentAvailable { get; set; }
        public bool IsSelected { get; set; }

    }
}
