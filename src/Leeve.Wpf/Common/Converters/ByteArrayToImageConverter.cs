using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Leeve.Wpf.Common.Converters;

public sealed class ByteArrayToImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value is not byte[] bytes) return null;

            using var memoryStream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = memoryStream;

            var decodeSize = GetSize(parameter?.ToString());
            if (decodeSize != null)
            {
                image.DecodePixelHeight = (int) decodeSize.Value.Height;
                image.DecodePixelWidth = (int) decodeSize.Value.Width;
            }

            image.EndInit();
            image.Freeze();
            memoryStream.Dispose();

            return image;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static Size? GetSize(string? parameter)
    {
        try
        {
            if (parameter is null) return null;
            var param = parameter.Split(",");
            return new Size(int.Parse(param[0]), int.Parse(param[1]));
        }
        catch (Exception)
        {
            throw new ArgumentException("Invalid parameter");
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}