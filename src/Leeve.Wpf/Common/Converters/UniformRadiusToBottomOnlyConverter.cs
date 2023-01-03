using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Leeve.Wpf.Common.Converters;

public sealed class UniformRadiusToBottomOnlyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return new CornerRadius();

        var radius = (double) value;
        return new CornerRadius(0, 0, radius, radius);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}