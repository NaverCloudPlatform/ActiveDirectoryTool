using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfApp1.Core;

namespace WpfApp1
{
    public class BubbleContentViewModel : BaseViewModel
    {
        public Brush BubbleBackground{ get; set; }
        public Control Content { get; set; }
        public HorizontalAlignment ArrowAlignment { get; set; }

        public BubbleContentViewModel()
        {
            //리소스키로 리소스를 찾고 브러시형으로 바꾸어 씀
            BubbleBackground = Application.Current.FindResource("ForegroundLightBrush") as Brush;
            ArrowAlignment = HorizontalAlignment.Left; 
        }

    }
}
