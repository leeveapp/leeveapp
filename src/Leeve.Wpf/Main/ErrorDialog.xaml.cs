using System;
using System.Media;
using System.Windows;

namespace Leeve.Wpf.Main;

public partial class ErrorDialog
{
    public ErrorDialog(string message)
    {
        InitializeComponent();
        ErrorMessage.Text = "Error message:\n" +
                            $"{message}\n\n" +
                            "We apologize for the inconvenience that this may cause.\n" +
                            "Please see log files or contact app developers for more details.\n\n" +
                            "System is shutting down...";

        SystemSounds.Asterisk.Play();
        Closed += (_, _) => Environment.Exit(1);
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        Close();
    }
}