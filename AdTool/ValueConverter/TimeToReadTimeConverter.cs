using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AdTool
{

    public class TimeToReadTimeConverter : BaseValueConverter<TimeToReadTimeConverter>
    {        
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var time = (DateTimeOffset)value;

            if (time == DateTimeOffset.MinValue)
                return string.Empty;

            if (time.Date == DateTimeOffset.UtcNow.Date)
                return $"Read {time.ToLocalTime().ToString("HH:mm")}";

            return $"Read {time.ToLocalTime().ToString("HH:mm MMM yyyy")}";
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
