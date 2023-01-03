namespace Leeve.Core.Common;

public interface IBrowserDialog
{
    string Path { get; }
    bool BrowseFile(string title, string filter, bool multiSelect = false);
    bool SaveFile(string title, string filter, string filename);
}