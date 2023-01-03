using Leeve.Core.Common;
using Microsoft.Win32;

namespace Leeve.Wpf.Common;

public sealed class BrowserDialog : IBrowserDialog
{
    public string Path { get; private set; } = string.Empty;

    public bool BrowseFile(string title, string filter, bool multiSelect = false)
    {
        var dialog = new OpenFileDialog { Title = title, Filter = filter, Multiselect = multiSelect };
        var selected = dialog.ShowDialog();

        if (selected == true) Path = dialog.FileName;

        return selected == true;
    }

    public bool SaveFile(string title, string filter, string filename)
    {
        var dialog = new SaveFileDialog { Title = title, Filter = filter, FileName = filename };
        var selected = dialog.ShowDialog();

        if (selected == true) Path = dialog.FileName;

        return selected == true;
    }
}