using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Leeve.Wpf.Common.Converters;

public sealed class CornerRadiusToUniformConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return new CornerRadius();

        var radius = (CornerRadius) value;
        var max = Math.Max(radius.TopLeft, Math.Max(radius.TopRight, Math.Max(radius.BottomLeft, radius.BottomRight)));
        return max;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}