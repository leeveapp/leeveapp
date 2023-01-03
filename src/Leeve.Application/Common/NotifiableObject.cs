namespace Leeve.Application.Common;

public class NotifiableObject : ObservableObject
{
    /// <summary>
    ///     Force to invoke property changed event.
    /// </summary>
    /// <param name="propertyName"></param>
    public void NotifyPropertyChanged(string propertyName)
    {
        OnPropertyChanged(propertyName);
    }
}