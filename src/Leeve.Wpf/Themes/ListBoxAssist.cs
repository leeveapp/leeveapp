using System;
using System.Windows;
using System.Windows.Controls;

namespace Leeve.Wpf.Themes;

public sealed class ListBoxAssist
{
    public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.RegisterAttached(
        "AutoScroll", typeof(bool), typeof(ListBox),
        new PropertyMetadata(default(bool), OnIsAutoScrollChanged));

    private static void OnIsAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var autoScroll = (bool) e.NewValue;
        if (!autoScroll) return;

        if (d is ListBox listBox)
            listBox.SelectionChanged += ListBoxOnSelectionChanged;
    }

    private static void ListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox) return;

        if (listBox.SelectedItem != null)
            listBox.Dispatcher?.BeginInvoke((Action) (() =>
            {
                listBox.UpdateLayout();
                if (listBox.SelectedItem != null)
                    listBox.ScrollIntoView(listBox.SelectedItem);
            }));
    }

    public static void SetAutoScroll(DependencyObject element, bool value)
    {
        element.SetValue(AutoScrollProperty, value);
    }

    public static bool GetAutoScroll(DependencyObject element) => (bool) element.GetValue(AutoScrollProperty);
}