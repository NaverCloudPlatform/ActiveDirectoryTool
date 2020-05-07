using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AdTool.Core;
using LogClient;
namespace AdTool
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ApplicationSetup();

            DataManager dataManager = DataManager.Instance;
            LogClient.Config logClientConfig = LogClient.Config.Instance;

            dataManager.LoadUserData();

            Current.MainWindow = new MainWindow();
            Current.MainWindow.Show();
        }

        private void ApplicationSetup()
        {
            IoC.Setup();
            IoC.Kernel.Bind<IUIManager>().ToConstant(new UIManager());
        }
    }
}
