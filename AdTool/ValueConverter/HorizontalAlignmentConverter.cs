using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AdTool.Core;

namespace AdTool
{

    public class HorizontalAlignmentConverter : BaseValueConverter<HorizontalAlignmentConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            #region 
            //var realVal = (ElementHorizontalAlignment)value;
            //switch (realVal)
            //{
            //    case ElementHorizontalAlignment.Left:
            //        return HorizontalAlignment.Left;
            //}
            #endregion
            return (HorizontalAlignment)value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
