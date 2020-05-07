using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AdTool.Core
{
    public class ConfigListDesignModel : ConfigListViewModel
    {

        private static readonly Lazy<ConfigListDesignModel> lazy =
            new Lazy<ConfigListDesignModel>(() => new ConfigListDesignModel(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static ConfigListDesignModel Instance { get { return lazy.Value; } }

        public ConfigListDesignModel() 
        {
            Items = new ObservableCollection<ConfigListItemViewModel>
            {
                new ConfigListItemViewModel
                {
                    Name = "ObjectStorage",
                    Number = "C1",
                    Message = "ObjectStorage Setting",
                    ProfilePictureRGB = "5c5c5c",
                    NewContentAvailable = true,
                    IsSelected = true,
                },
                new ConfigListItemViewModel
                {
                    Name = "LoginKey",
                    Number = "C2",
                    Message = "LoginKey Setting",
                    ProfilePictureRGB = "5c5c5c",
                },
                //new ConfigListItemViewModel
                //{
                //    Name = "InitScript",
                //    Number = "C3",
                //    Message = "InitScript Setting & Upload",
                //    ProfilePictureRGB = "5c5c5c",                    
                //},
                new ConfigListItemViewModel
                {
                    Name = "ConfigCheck",
                    Number = "C3",
                    Message = "User Configuration Check",
                    ProfilePictureRGB = "fe4503",  
                },
            };
        }
    }
}
