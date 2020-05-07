using System.Windows;

namespace AdTool
{
    public abstract class AnimateBaseProperty<Parent> : BaseAttachedProperty<Parent, bool> 
        where Parent : BaseAttachedProperty<Parent, bool>, new()
    {
        public bool FirstLoad { get; set; } = true;

        public override void OnValueUpdated(DependencyObject sender, object value)
        {
            if (!(sender is FrameworkElement element))
                return;

            if (sender.GetValue(ValueProperty) == value && !FirstLoad)
                return;

            if (FirstLoad)
            {
                RoutedEventHandler onLoaded = null; 
                onLoaded = (ss, ee) =>
                {
                    element.Loaded -= onLoaded;
                    DoAnimation(element, (bool)value);
                    FirstLoad = false;
                };
                element.Loaded += onLoaded;
            }
            else
                DoAnimation(element, (bool)value);
        }

        protected virtual void DoAnimation(FrameworkElement element, bool value) { }
    }

    public class AnimateSlideInFromLeftProperty : AnimateBaseProperty<AnimateSlideInFromLeftProperty>
    {
        protected override async void DoAnimation(FrameworkElement element, bool value) 
        {
            if (value)
                await element.SlideAndFadeInFromLeftAsync(FirstLoad ? 0 : 0.3f, keepMargin: false); 
            else
                await element.SlideAndFadeOutToLeftAsync(FirstLoad ? 0 : 0.3f, keepMargin: false);

        }
    }

    public class AnimateSlideInFromBottomProperty : AnimateBaseProperty<AnimateSlideInFromBottomProperty>
    {
        protected override async void DoAnimation(FrameworkElement element, bool value) 
        {
            if (value)
                await element.SlideAndFadeInFromBottomAsync(FirstLoad ? 0 : 0.3f, keepMargin: false); 
            else
                await element.SlideAndFadeOutToBottomAsync(FirstLoad ? 0 : 0.3f, keepMargin: false);
        }
    }

    public class AnimateFadeInProperty : AnimateBaseProperty<AnimateFadeInProperty>
    {
        protected override async void DoAnimation(FrameworkElement element, bool value)
        {
            if (value)
                await element.FadeInAsync(FirstLoad ? 0 : 0.3f);
            else
                await element.FadeOutAsync(FirstLoad ? 0 : 0.3f);
        }
    }
}
