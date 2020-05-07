using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Animation;
using AdTool.Core;
using System.ComponentModel;
using System.Diagnostics;

namespace AdTool
{
    public class BasePage : UserControl 
    {
        public PageAnimation PageLoadAnimation { get; set; } = PageAnimation.SlideAndFadeInFromRight;
        public PageAnimation PageUnLoadAnimation { get; set; } = PageAnimation.SlideAndFadeOutToLeft;
        public float SlideSeconds { get; set; } = 0.3f;
        public bool ShouldAnimateOut { get; set; }

        public BasePage() 
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return; 

            if (PageLoadAnimation != PageAnimation.None)
                Visibility = Visibility.Collapsed;
            Loaded += BasePage_LoadedAsync;
        }
        private async void BasePage_LoadedAsync(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ShouldAnimateOut)
                await AnimateOutAsync();

            else
                await AnimateInAsync();
        }
        public async Task AnimateInAsync()
        {
            if (PageLoadAnimation == PageAnimation.None)
                return;

            switch (PageLoadAnimation)
            {
                case PageAnimation.SlideAndFadeInFromRight:
                    await this.SlideAndFadeInFromRightAsync(SlideSeconds, width: (int)Application.Current.MainWindow.Width);
                    break;
            }
        }

        public async Task AnimateOutAsync()
        {
            if (PageUnLoadAnimation == PageAnimation.None)
                return;

            switch (PageUnLoadAnimation)
            {
                case PageAnimation.SlideAndFadeOutToLeft:
                    await this.SlideAndFadeOutToLeftAsync(SlideSeconds);
                    break;
            }
        }
    }

    public class BasePage<VM> : BasePage 
        where VM : BaseViewModel, new()
    {
        private VM mViewModel; 

        public BasePage() : base()
        {
            ViewModel = new VM();
        }

        public VM ViewModel 
        { 
            get => mViewModel; 
            set
            {
                if (mViewModel == value)
                    return;

                mViewModel = value;
                if (value is SetAdPrimaryViewModel)
                    DataContext = SetAdPrimaryViewModel.Instance;
                else if (value is SetAdSecondaryViewModel)
                    DataContext = SetAdSecondaryViewModel.Instance;
                else if (value is ConfigCheckViewModel)
                    DataContext = ConfigCheckViewModel.Instance;
                else
                    DataContext = mViewModel;
            }
        }
    }


}
