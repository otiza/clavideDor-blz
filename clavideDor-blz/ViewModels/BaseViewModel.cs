using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace clavideDor_blz.ViewModels;

/// <summary>
/// Base ViewModel class implementing INotifyPropertyChanged for data binding
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void ClearError()
    {
        ErrorMessage = string.Empty;
    }

    protected void SetError(string message)
    {
        ErrorMessage = message;
    }
}

