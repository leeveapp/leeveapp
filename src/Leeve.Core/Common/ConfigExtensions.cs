namespace Leeve.Core.Common;

public static class ConfigExtensions
{
    public static async Task<bool> SaveAsync<T>(this T cfg, string filePath)
    {
        try
        {
            var info = JsonSerializer.Serialize(cfg);

            if (!File.Exists(filePath))
            {
                await File.WriteAllTextAsync(filePath, info).ConfigureAwait(false);
                return true;
            }

            var config = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            var parsed = config.TryParseAsJsonDocument(out _);

            if (parsed)
            {
                var merge = config.MergeJson(info);
                await File.WriteAllTextAsync(filePath, merge).ConfigureAwait(false);
            }
            else
            {
                await File.WriteAllTextAsync(filePath, info).ConfigureAwait(false);
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}