using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdTool.Core
{
    public class ServerListItemDesignModel : ServerListItemViewModel
    {
        public static ServerListItemDesignModel Instance => new ServerListItemDesignModel();

        public ServerListItemDesignModel() 
        {
            Number = "S1";
            Name = "CreateServer";
            Message = "Create Server Menu";
            ProfilePictureRGB = "00d405";
            NewContentAvailable = true;
        }
    }
}
