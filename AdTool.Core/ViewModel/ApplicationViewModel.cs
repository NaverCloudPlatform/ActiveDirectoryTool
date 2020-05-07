using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AdTool.Core
{
    public class ApplicationViewModel : BaseViewModel
    {
        #region command
        
        public ICommand OpenServerCommand { get; set; }
        public ICommand OpenConfigCommand { get; set; }
        #endregion

        #region property
        public ApplicationPage CurrentPage { get; set; } = ApplicationPage.Login;
        public SideMenuContent CurrentSideMenuContent { get; set; } = SideMenuContent.Config;
        public bool SideMenuVisible { get; set; } = false;
        #endregion

        #region constructor
        public ApplicationViewModel()
        {
            OpenServerCommand = new RelayCommand(OpenServer);
            OpenConfigCommand = new RelayCommand(OpenConfig);
        }
        #endregion

        #region methods
        public void OpenConfig()
        {
            CurrentSideMenuContent = SideMenuContent.Config;
        }

        public void OpenServer()
        {
            CurrentSideMenuContent = SideMenuContent.Server;
        }

        public void GoToPage(ApplicationPage page)
        {
            SideMenuVisible = true;
            CurrentPage = page;
        }
        #endregion
    }
}
