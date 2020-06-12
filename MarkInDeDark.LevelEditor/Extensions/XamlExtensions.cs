using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace WPFClient.Extensions
{
    public static class XamlExtensions
    {
        public static void FadeIn(
            this FrameworkElement control,
            int durationMilliseconds = 400,
            Action onCompleted = null,
            double maxOpacity = 1)
        {
            var animation = new DoubleAnimation
            {
                To = maxOpacity,
                From = null,
                EasingFunction = new ExponentialEase(),
                Duration = TimeSpan.FromMilliseconds(durationMilliseconds)
            };
            
            control.BeginAnimation(FrameworkElement.OpacityProperty, animation);

            if (onCompleted != null)
            {
                animation.Completed += (sender, e) =>
                {
                    onCompleted();
                };
            }
        }

        public static void FadeOut(
            this FrameworkElement control,
            int durationMilliseconds = 400,
            Action onCompleted = null)
        {
            var animation = new DoubleAnimation
            {
                To = 0,
                From = null,
                EasingFunction = new ExponentialEase(),
                Duration = TimeSpan.FromMilliseconds(durationMilliseconds)
            };

            if (onCompleted != null)
            {
                animation.Completed += (sender, e) =>
                {
                    onCompleted();
                };
            }

            control.BeginAnimation(UIElement.OpacityProperty, animation);
        }
    }
}

