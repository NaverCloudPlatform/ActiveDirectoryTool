using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdTool.Core
{
    public interface IUIManager
    {
        Task ShowMessage(MessageBoxDialogViewModel viewModel);
        Task ShowMessage(MessageBoxDialogConfirmViewModel viewModel);
    }
}
