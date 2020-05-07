using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdTool.Core
{
    public class ConfigListItemDesignModel : ConfigListItemViewModel
    {
        public static ConfigListItemDesignModel Instance => new ConfigListItemDesignModel();

        public ConfigListItemDesignModel() 
        {
            Number = "C1";
            Name = "ObjectStorage";
            Message = "ObjectStorage Setting";
            ProfilePictureRGB = "00d405";
            NewContentAvailable = true;
            IsSelected = true;
        }
    }
}
