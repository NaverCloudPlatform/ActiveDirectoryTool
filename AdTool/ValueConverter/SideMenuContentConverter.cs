using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdTool.Core;

namespace AdTool
{
    public class SideMenuContentConverter : BaseValueConverter<SideMenuContentConverter>
    {
        protected ServerListControl mServerListControl = new ServerListControl();
        protected ConfigListControl mConfigListControl = new ConfigListControl();

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sideMenuType = (SideMenuContent)value;

            switch (sideMenuType)
            {
                case SideMenuContent.Server:
                    return mServerListControl;

                case SideMenuContent.Config:
                    return mConfigListControl;
                // Unknown
                default:
                    return "No UI";
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
