using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityVoting.Maui.Services;

namespace CommunityVoting.Maui.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private string _email = "juan@comunidad.com";

    [ObservableProperty]
    private string _password = "Voter123!";

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStandardLoginMode))]
    private bool _isResetMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStandardLoginMode))]
    private bool _isTokenAccessMode;

    public bool IsStandardLoginMode => !IsResetMode && !IsTokenAccessMode;

    [ObservableProperty]
    private string _tokenOrUrl = string.Empty;

    [ObservableProperty]
    private string _accessCode = string.Empty;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private void ToggleResetMode()
    {
        IsResetMode = !IsResetMode;
        IsTokenAccessMode = false;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    [RelayCommand]
    private void ToggleTokenAccessMode()
    {
        IsTokenAccessMode = !IsTokenAccessMode;
        IsResetMode = false;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    [RelayCommand]
    private async Task LoginWithTokenAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(TokenOrUrl) || string.IsNullOrWhiteSpace(AccessCode))
        {
            ErrorMessage = "Por favor, introduce el enlace/token y el código de 6 dígitos.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var (success, errorMsg) = await _authService.MeetingAccessLoginAsync(TokenOrUrl, AccessCode);
            if (success)
            {
                await Shell.Current.GoToAsync("//MeetingsPage");
            }
            else
            {
                ErrorMessage = errorMsg ?? "No se pudo acceder a la reunión. Comprueba el código.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor, introduce email y contraseña.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            bool success = await _authService.LoginAsync(Email, Password);
            if (success)
            {
                await Shell.Current.GoToAsync("//MeetingsPage");
            }
            else
            {
                ErrorMessage = "Credenciales incorrectas o error de conexión.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = "Introduce tu email y la nueva contraseña deseada.";
            return;
        }

        if (NewPassword.Length < 6)
        {
            ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            bool success = await _authService.ResetPasswordAsync(Email, NewPassword);
            if (success)
            {
                SuccessMessage = "¡Contraseña cambiada con éxito! Ya puedes iniciar sesión.";
                Password = NewPassword;
                IsResetMode = false;
            }
            else
            {
                ErrorMessage = "No se pudo restablecer la contraseña. Verifica tu correo.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
