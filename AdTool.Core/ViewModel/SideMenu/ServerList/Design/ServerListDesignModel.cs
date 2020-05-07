using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace AdTool.Core
{
    public class ServerListDesignModel : ServerListViewModel
    {

        private static readonly Lazy<ServerListDesignModel> lazy =
            new Lazy<ServerListDesignModel>(() => new ServerListDesignModel(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static ServerListDesignModel Instance { get { return lazy.Value; } }

        public ServerListDesignModel() 
        {
            Items = new ObservableCollection<ServerListItemViewModel>
            {
                new ServerListItemViewModel
                {
                    Name = "CreateServer",
                    Number = "S1",
                    Message = "Create Server",
                    ProfilePictureRGB = "5c5c5c",
                    NewContentAvailable = true,
                    IsSelected = true,
                },
                new ServerListItemViewModel
                {
                    Name = "CreateIp",
                    Number = "S2",
                    Message = "Create Public Ip and Server Management",
                    ProfilePictureRGB = "5c5c5c"
                },
                new ServerListItemViewModel
                {
                    Name = "SetAgentKey",
                    Number = "S3",
                    Message = "Set AccessKey and Secret Key for Agent",
                    ProfilePictureRGB = "5c5c5c",

                },
                new ServerListItemViewModel
                {
                    Name = "SetAdGroup",
                    Number = "S4",
                    Message = "Set Active Directory Group",
                    ProfilePictureRGB = "fe4503",
                },
                new ServerListItemViewModel
                {
                    Name = "SetAdPrimary",
                    Number = "S5",
                    Message = "Set Active Directory Primary Server",
                    ProfilePictureRGB = "fe4503",
                },
                new ServerListItemViewModel
                {
                    Name = "SetAdSecondary",
                    Number = "S6",
                    Message = "Set Active Directory Secondary Server",
                    ProfilePictureRGB = "fe4503"
                },
            };
        }
    }
}
