using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AdTool.Core;

namespace AdTool
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, IWindowFacade
    {
        //public ApplicationViewModel ApplicationViewModel => new ApplicationViewModel(); 
        //이거 대신 IoC 이용
        //19장52분부터 다시 볼 것
         
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);
        }

        private void AppWindow_Deactivated(object sender, EventArgs e)
        {
            (DataContext as WindowViewModel).DimmableOverlayVisible = true;
        }

        private void AppWindow_Activated(object sender, EventArgs e)
        {
            (DataContext as WindowViewModel).DimmableOverlayVisible = false;
        }
    }
}
