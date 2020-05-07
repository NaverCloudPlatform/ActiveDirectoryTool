using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdTool.Core
{
    public class MessageBoxDialogConfirmDesignModel : MessageBoxDialogViewModel
    {
        public static MessageBoxDialogConfirmDesignModel Instance => new MessageBoxDialogConfirmDesignModel();        

        public MessageBoxDialogConfirmDesignModel()
        {
            OkText = "OK";
            Message = "Message";
        }

    }
}
