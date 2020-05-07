using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdTool.Core;

namespace AdTool
{
    class ApplicationPageValueConverter : BaseValueConverter<ApplicationPageValueConverter>
    {
        public override object Convert(object value, Type targetType = null, object parameter = null, CultureInfo culture = null)
        {
            switch ((ApplicationPage)value)
            {
                case ApplicationPage.Login:
                    return new LoginPage();
                case ApplicationPage.ObjectStorage:
                    return new ObjectStoragePage();
                case ApplicationPage.LoginKey:
                    return new LoginKeyPage();
                case ApplicationPage.InitScript:
                    return new InitScriptPage();
                case ApplicationPage.ConfigCheck:
                    return new ConfigCheckPage();
                case ApplicationPage.CreateServer:
                    return new CreateServerPage();
                case ApplicationPage.CreateIp:
                    return new CreateIpPage();
                case ApplicationPage.SetAgentKey:
                    return new SetAgentKeyPage();
                case ApplicationPage.SetAdGroup:
                    return new SetAdGroupPage();
                case ApplicationPage.SetAdPrimary:
                    return new SetAdPrimaryPage();
                case ApplicationPage.SetAdSecondary:
                    return new SetAdSecondaryPage();

                default:
                    Debugger.Break();
                    return null;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
