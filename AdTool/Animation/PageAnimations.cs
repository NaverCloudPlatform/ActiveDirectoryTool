using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace AdTool
{
    public static class PageAnimations
    {
        public static async Task SlideAndFadeInFromRightAsync(this Page page, float seconds)
        {
            var sb = new Storyboard();
            sb.AddSlideFromRight(seconds, page.WindowWidth);
            sb.AddFadeIn(seconds);
            sb.Begin(page);
            page.Visibility = Visibility.Visible;
            await Task.Delay((int)(seconds * 1000));
        }

        public static async Task SlideAndFadeOutToLeftAsync(this Page page, float seconds)
        {
            var sb = new Storyboard();
            sb.AddSlideToLeft(seconds, page.WindowWidth);
            sb.AddFadeOut(seconds);
            sb.Begin(page);
            page.Visibility = Visibility.Visible;
            await Task.Delay((int)(seconds * 1000));
        }
    }
}
