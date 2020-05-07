using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdTool.Core;

namespace AdTool
{
    public class BaseDialogUserControl : UserControl
    {
        private DialogWindow mDialogWindow;

        public int WindowMinimumWidth { get; set; } = 250;
        public int WindowMinimumHeight { get; set; } = 100;
        public string Title { get; set; }
        public int TitleHeight { get; set; } = 42;
        public ICommand CloseCommand { get; private set; }
        public BaseDialogUserControl()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                mDialogWindow = new DialogWindow();
                mDialogWindow.ViewModel = new DialogWindowViewModel(mDialogWindow);
                CloseCommand = new RelayCommand(() => mDialogWindow.Close());
            }
        }
        public Task ShowDialog<T>(T viewModel)
            where T : BaseDialogViewModel
        {
            var tcs = new TaskCompletionSource<bool>();
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    mDialogWindow.ViewModel.WindowMinimumWidth = WindowMinimumWidth;
                    mDialogWindow.ViewModel.WindowMinimumHeight = WindowMinimumHeight;
                    mDialogWindow.ViewModel.TitleHeight = TitleHeight;
                    mDialogWindow.ViewModel.Title = string.IsNullOrEmpty(viewModel.Title) ? Title : viewModel.Title;
                    mDialogWindow.ViewModel.Content = this;
                    DataContext = viewModel;
                    mDialogWindow.ShowDialog();
                }
                finally
                {
                    tcs.TrySetResult(true);
                }
            });

            return tcs.Task;
        }

    }
}
