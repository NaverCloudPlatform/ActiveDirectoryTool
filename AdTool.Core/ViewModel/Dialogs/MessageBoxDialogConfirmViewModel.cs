using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AdTool.Core
{
    public class MessageBoxDialogConfirmViewModel : BaseDialogViewModel
    {
        public ICommand ConfirmYesCommand { get; set; }
        public string Message { get; set; }
        public DialogResult DialogResult { get; set; } = DialogResult.No;

        public MessageBoxDialogConfirmViewModel()
        {
            ConfirmYesCommand = new RelayParameterizedCommand((parameter) => ConfirmYes(parameter));
        }

        private void ConfirmYes(object parameter)
        {
            DialogResult = DialogResult.Yes;
            var windowFacade = parameter as IWindowFacade;
            windowFacade?.Close();
        }
    }

    
}
