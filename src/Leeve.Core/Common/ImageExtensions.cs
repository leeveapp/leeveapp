namespace Leeve.Core.Common;

public static class ImageExtensions
{
    public static Task<byte[]> ToImageByteArrayAsync(this string imagePath, int maxWidth, int maxHeight, int quality = 100,
        int maxSize = 2 * 1024 * 1024)
    {
        return Task.Factory.StartNew(() => ToImageByteArray(imagePath, maxWidth, maxHeight, quality, maxSize));
    }

    private static byte[] ToImageByteArray(this string imagePath, int maxWidth, int maxHeight, int quality, int maxSize)
    {
        var bytes = File.ReadAllBytes(imagePath);

        if (bytes.Length > maxSize) throw new ArgumentOutOfRangeException(nameof(maxSize));

        using var inStream = new MemoryStream(bytes);
        using var outStream = new MemoryStream();
        using var image = Image.Load(inStream);

        var width = maxWidth < image.Width ? maxWidth : image.Width;
        var height = maxHeight < image.Height ? maxHeight : image.Height;

        var jpegEncoder = new JpegEncoder { Quality = quality };
        using var clone = image.Clone(context => context
            .Resize(new ResizeOptions
            {
                Mode = ResizeMode.Crop,
                Size = new Size(width, height)
            }));
        clone.Save(outStream, jpegEncoder);
        return outStream.ToArray();
    }
}