
using System.Threading.Tasks;
using AdTool.Core; 

namespace AdTool
{
    public class UIManager : IUIManager
    {
        public Task ShowMessage(MessageBoxDialogViewModel viewModel)
        {
            return new DialogMessageBox().ShowDialog(viewModel);
        }

        public Task ShowMessage(MessageBoxDialogConfirmViewModel viewModel)
        {
            return new DialogConfirmMessageBox().ShowDialog(viewModel);
        }

    }
}
