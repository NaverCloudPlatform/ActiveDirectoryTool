using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdTool
{
    public class BooleanToBorderThicknessConverter : BaseValueConverter<BooleanToBorderThicknessConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return (bool)value ? 2 : 0;
            else
                return (bool)value ? 0 : 2;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
