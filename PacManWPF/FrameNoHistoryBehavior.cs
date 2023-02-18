using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PacManWPF
{
    public class FrameNoHistoryBehavior
    {
        public static bool GetEnable(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableProperty);
        }

        public static void SetEnable(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableProperty, value);
        }

        // Using a DependencyProperty as the backing store for Enable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached("Enable",
                typeof(bool),
                typeof(FrameNoHistoryBehavior),
                new UIPropertyMetadata(false, OnEnablePropertyChanged));

        private static void OnEnablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Frame frame))
                throw new NotSupportedException($"This behavior supports only {nameof(Frame)}");

            if ((bool)e.NewValue)
            {
                frame.Navigated += OnFrameNavigated;
            }
            else
            {
                frame.Navigated -= OnFrameNavigated;
            }

            void OnFrameNavigated(object sender, NavigationEventArgs e)
            {
                frame.NavigationService.RemoveBackEntry();
            }
        }
    }
}
