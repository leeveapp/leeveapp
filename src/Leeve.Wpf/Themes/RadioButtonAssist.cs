using System.Windows;
using System.Windows.Media;

namespace Leeve.Wpf.Themes;

public sealed class RadioButtonAssist
{
    public static readonly DependencyProperty MouseOverBackgroundProperty = DependencyProperty.RegisterAttached(
        "MouseOverBackground", typeof(SolidColorBrush), typeof(RadioButtonAssist),
        new PropertyMetadata(default(SolidColorBrush)));

    /// <summary>
    ///     Background color of element on mouse over.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="value"></param>
    public static void SetMouseOverBackground(DependencyObject element, SolidColorBrush value)
    {
        element.SetValue(MouseOverBackgroundProperty, value);
    }

    public static SolidColorBrush GetMouseOverBackground(DependencyObject element) =>
        (SolidColorBrush) element.GetValue(MouseOverBackgroundProperty);

    public static readonly DependencyProperty MouseOverForegroundProperty = DependencyProperty.RegisterAttached(
        "MouseOverForeground", typeof(SolidColorBrush), typeof(RadioButtonAssist),
        new PropertyMetadata(default(SolidColorBrush)));

    /// <summary>
    ///     Foreground color of element on mouse over.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="value"></param>
    public static void SetMouseOverForeground(DependencyObject element, SolidColorBrush value)
    {
        element.SetValue(MouseOverForegroundProperty, value);
    }

    public static SolidColorBrush GetMouseOverForeground(DependencyObject element) =>
        (SolidColorBrush) element.GetValue(MouseOverForegroundProperty);

    public static readonly DependencyProperty MouseOverBorderProperty = DependencyProperty.RegisterAttached(
        "MouseOverBorder", typeof(SolidColorBrush), typeof(RadioButtonAssist),
        new PropertyMetadata(default(SolidColorBrush)));

    /// <summary>
    ///     Border color of element on mouse over.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="value"></param>
    public static void SetMouseOverBorder(DependencyObject element, SolidColorBrush value)
    {
        element.SetValue(MouseOverBorderProperty, value);
    }

    public static SolidColorBrush GetMouseOverBorder(DependencyObject element) =>
        (SolidColorBrush) element.GetValue(MouseOverBorderProperty);

    public static readonly DependencyProperty IsCheckedBackgroundProperty = DependencyProperty.RegisterAttached(
        "IsCheckedBackground", typeof(SolidColorBrush), typeof(RadioButtonAssist),
        new PropertyMetadata(default(SolidColorBrush)));

    /// <summary>
    ///     Background color of element when selected.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="value"></param>
    public static void SetIsCheckedBackground(DependencyObject element, SolidColorBrush value)
    {
        element.SetValue(IsCheckedBackgroundProperty, value);
    }

    public static SolidColorBrush GetIsCheckedBackground(DependencyObject element) =>
        (SolidColorBrush) element.GetValue(IsCheckedBackgroundProperty);

    public static readonly DependencyProperty IsCheckedForegroundProperty = DependencyProperty.RegisterAttached(
        "IsCheckedForeground", typeof(SolidColorBrush), typeof(RadioButtonAssist),
        new PropertyMetadata(default(SolidColorBrush)));

    /// <summary>
    ///     Foreground color of element when selected.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="value"></param>
    public static void SetIsCheckedForeground(DependencyObject element, SolidColorBrush value)
    {
        element.SetValue(IsCheckedForegroundProperty, value);
    }

    public static SolidColorBrush GetIsCheckedForeground(DependencyObject element) =>
        (SolidColorBrush) element.GetValue(IsCheckedForegroundProperty);

    public static readonly DependencyProperty IsCheckedBorderProperty = DependencyProperty.RegisterAttached(
        "IsCheckedBorder", typeof(SolidColorBrush), typeof(RadioButtonAssist),
        new PropertyMetadata(default(SolidColorBrush)));

    /// <summary>
    ///     Border color of element when selected.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="value"></param>
    public static void SetIsCheckedBorder(DependencyObject element, SolidColorBrush value)
    {
        element.SetValue(IsCheckedBorderProperty, value);
    }

    public static SolidColorBrush GetIsCheckedBorder(DependencyObject element) =>
        (SolidColorBrush) element.GetValue(IsCheckedBorderProperty);
}