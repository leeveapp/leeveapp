using System.Windows;

namespace Leeve.Wpf.Themes;

public sealed class CornerRadiusAssist
{
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
        "CornerRadius", typeof(CornerRadius), typeof(CornerRadiusAssist),
        new PropertyMetadata(default(CornerRadius)));

    public static void SetCornerRadius(DependencyObject element, CornerRadius value)
    {
        element.SetValue(CornerRadiusProperty, value);
    }

    public static CornerRadius GetCornerRadius(DependencyObject element) => (CornerRadius) element.GetValue(CornerRadiusProperty);
}