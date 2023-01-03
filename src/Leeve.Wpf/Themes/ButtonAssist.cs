using System.Windows;
using System.Windows.Input;

namespace Leeve.Wpf.Themes;

public sealed class ButtonAssist
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
        "Command", typeof(ICommand), typeof(ButtonAssist), new PropertyMetadata(default(ICommand)));

    public static void SetCommand(DependencyObject element, ICommand value)
    {
        element.SetValue(CommandProperty, value);
    }

    public static ICommand GetCommand(DependencyObject element) => (ICommand) element.GetValue(CommandProperty);

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
        "CommandParameter", typeof(object), typeof(ButtonAssist), new PropertyMetadata(default(object)));

    public static void SetCommandParameter(DependencyObject element, object value)
    {
        element.SetValue(CommandParameterProperty, value);
    }

    public static object GetCommandParameter(DependencyObject element) => element.GetValue(CommandParameterProperty);
}