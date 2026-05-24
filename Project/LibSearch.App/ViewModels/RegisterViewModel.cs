using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibSearch.App.Services;

namespace LibSearch.App.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly ISessionService _session;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _passwordConfirm = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    public RegisterViewModel(IAuthService auth, ISessionService session)
    {
        _auth = auth;
        _session = session;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            if (Password != PasswordConfirm)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }
            var (ok, error, user) = await _auth.RegisterAsync(Username, Password);
            if (!ok || user is null)
            {
                ErrorMessage = error;
                return;
            }
            _session.SetUser(user);
            Password = string.Empty;
            PasswordConfirm = string.Empty;
            await Shell.Current.GoToAsync("//library");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("//login");
    }
}
