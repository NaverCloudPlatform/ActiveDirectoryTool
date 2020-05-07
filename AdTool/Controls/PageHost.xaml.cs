using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// PageHost.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PageHost : UserControl
    {
        
        public BasePage CurrentPage 
        {
            get => (BasePage)GetValue(CurrentPageProperty); 
            set => SetValue(CurrentPageProperty, value);
        }

        
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(nameof(CurrentPage), 
                typeof(BasePage), 
                typeof(PageHost), 
                new UIPropertyMetadata(CurrentPagePropertyChanged));

        private static void CurrentPagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newPageFrame = (d as PageHost).NewPage;
            var oldPageFrame = (d as PageHost).OldPage;
            var oldPageContent = newPageFrame.Content;
            newPageFrame.Content = null;
            oldPageFrame.Content = oldPageContent;
            if (oldPageContent is BasePage oldPage)
            {
                oldPage.ShouldAnimateOut = true;
             
                Task.Delay((int)(oldPage.SlideSeconds * 1000)).ContinueWith((t)=>
                {
                    Application.Current.Dispatcher.Invoke(()=> oldPageFrame.Content = null); 
                });
            }
            newPageFrame.Content = e.NewValue;
        }

        public PageHost()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
                NewPage.Content = (BasePage)new ApplicationPageValueConverter().Convert(IoC.Get<ApplicationViewModel>().CurrentPage);
        }
    }
}
