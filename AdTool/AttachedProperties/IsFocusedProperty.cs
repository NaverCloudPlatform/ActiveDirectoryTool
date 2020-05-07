using System.Windows;
using System.Windows.Controls;

namespace AdTool
{
    public class IsFocusedProperty : BaseAttachedProperty<IsFocusedProperty, bool>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is Control control))
                return;
            control.Loaded += (s, ee) => control.Focus();
        }

    }
}
