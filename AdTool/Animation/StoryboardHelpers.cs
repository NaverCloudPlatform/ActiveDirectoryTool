using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace AdTool
{
    public static class StoryboardHelpers
    {
        public static void AddSlideFromRight(this Storyboard storyboard, float seconds, double offset, float decelerationRatio = 0.9f, bool keepMargin = true)
        {
            
            var animation = new ThicknessAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(seconds)),
                From = new Thickness(keepMargin ? offset : 0, 0, -offset, 0), 
                To = new Thickness(0),
                DecelerationRatio = decelerationRatio
            };

            Storyboard.SetTargetProperty(animation, new PropertyPath("Margin"));
            storyboard.Children.Add(animation); 

        }

        public static void AddSlideFromLeft(this Storyboard storyboard, float seconds, double offset, float decelerationRatio = 0.9f, bool keepMargin = true)
        {

            var animation = new ThicknessAnimation 
            {
                Duration = new Duration(TimeSpan.FromSeconds(seconds)),
                From = new Thickness(-offset, 0, keepMargin ? offset : 0, 0), 
                To = new Thickness(0),
                DecelerationRatio = decelerationRatio
            };

            Storyboard.SetTargetProperty(animation, new PropertyPath("Margin"));
            storyboard.Children.Add(animation);

        }

        public static void AddSlideToLeft(this Storyboard storyboard, float seconds, double offset, float decelerationRatio = 0.9f, bool keepMargin = true)
        {

            var animation = new ThicknessAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(seconds)),
                From = new Thickness(0),
                To = new Thickness(-offset, 0, keepMargin ? offset : 0, 0), 
                DecelerationRatio = decelerationRatio,
            };

            Storyboard.SetTargetProperty(animation, new PropertyPath("Margin"));
            storyboard.Children.Add(animation);

        }

        public static void AddSlideFromBottom(this Storyboard storyboard, float seconds, double offset, float decelerationRatio = 0.9f, bool keepMargin = true)
        {

            var animation = new ThicknessAnimation 
            {
                Duration = new Duration(TimeSpan.FromSeconds(seconds)),
                From = new Thickness(0, keepMargin ? offset : 0, 0, -offset), 
                To = new Thickness(0),
                DecelerationRatio = decelerationRatio
            };

            Storyboard.SetTargetProperty(animation, new PropertyPath("Margin"));
            storyboard.Children.Add(animation);

        }

        public static void AddSlideToBottom(this Storyboard storyboard, float seconds, double offset, float decelerationRatio = 0.9f, bool keepMargin = true)
        {

            var animation = new ThicknessAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(seconds)),
                From = new Thickness(0),
                To = new Thickness(0, keepMargin ? offset : 0, 0, -offset), 
                DecelerationRatio = decelerationRatio,
            };

            Storyboard.SetTargetProperty(animation, new PropertyPath("Margin"));
            storyboard.Children.Add(animation);

        }

        public static void AddSlideToRight(this Storyboard storyboard, float seconds, double offset, float decelerationRatio = 0.9f, bool keepMargin = true)
        {

            var animation = new ThicknessAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(seconds)),
                From = new Thickness(0),
                To = new Thickness(keepMargin ? offset : 0, 0, -offset, 0), 
                DecelerationRatio = decelerationRatio,
            };

            Storyboard.SetTargetProperty(animation, new PropertyPath("Margin"));
            storyboard.Children.Add(animation);

        }

        public static void AddFadeIn(this Storyboard storyboard, float seconds)
        {

            var animation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(seconds)),
                From = 0,
                To = 1,
            };

            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
            storyboard.Children.Add(animation);

        }

        public static void AddFadeOut(this Storyboard storyboard, float seconds)
        {

            var animation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(seconds)),
                From = 1,
                To = 0,
            };

            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
            storyboard.Children.Add(animation);

        }
    }
}
