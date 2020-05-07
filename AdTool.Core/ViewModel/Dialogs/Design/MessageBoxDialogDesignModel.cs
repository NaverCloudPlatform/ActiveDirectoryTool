using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdTool.Core
{
    public class MessageBoxDialogDesignModel : MessageBoxDialogViewModel
    {
        public static MessageBoxDialogDesignModel Instance => new MessageBoxDialogDesignModel();        
        public MessageBoxDialogDesignModel()
        {
            OkText = "OK";
            Message = "Message";
        }

    }
}
