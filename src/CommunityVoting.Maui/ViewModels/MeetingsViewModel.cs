using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityVoting.Maui.Models;
using CommunityVoting.Maui.Services;

namespace CommunityVoting.Maui.ViewModels;

public partial class MeetingsViewModel : ObservableObject
{
    private readonly MeetingService _meetingService;
    private readonly AuthService _authService;

    [ObservableProperty]
    private ObservableCollection<MeetingItem> _meetings = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private bool _isChangePasswordModalVisible;

    [ObservableProperty]
    private string _oldPassword = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _passwordError = string.Empty;

    [ObservableProperty]
    private string _passwordSuccess = string.Empty;

    public MeetingsViewModel(MeetingService meetingService, AuthService authService)
    {
        _meetingService = meetingService;
        _authService = authService;
    }

    public async Task InitializeAsync()
    {
        UserName = _authService.GetUserName() ?? "Usuario";
        await LoadMeetingsAsync();
    }

    [RelayCommand]
    private void ToggleChangePasswordModal()
    {
        IsChangePasswordModalVisible = !IsChangePasswordModalVisible;
        OldPassword = string.Empty;
        NewPassword = string.Empty;
        PasswordError = string.Empty;
        PasswordSuccess = string.Empty;
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(OldPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            PasswordError = "Debes ingresar tu contraseña actual y la nueva.";
            return;
        }

        if (NewPassword.Length < 6)
        {
            PasswordError = "La nueva contraseña debe tener al menos 6 caracteres.";
            return;
        }

        try
        {
            IsBusy = true;
            PasswordError = string.Empty;
            PasswordSuccess = string.Empty;

            bool success = await _authService.ChangePasswordAsync(OldPassword, NewPassword);
            if (success)
            {
                PasswordSuccess = "¡Contraseña actualizada con éxito!";
                OldPassword = string.Empty;
                NewPassword = string.Empty;
            }
            else
            {
                PasswordError = "La contraseña actual no es correcta.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadMeetingsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var list = await _meetingService.GetMeetingsAsync();
            Meetings.Clear();
            foreach (var m in list)
            {
                Meetings.Add(m);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectMeetingAsync(MeetingItem meeting)
    {
        if (meeting == null) return;
        await Shell.Current.GoToAsync($"VotingPage?meetingId={meeting.Id}&title={Uri.EscapeDataString(meeting.Title)}");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        _authService.Logout();
        Meetings.Clear();
        UserName = string.Empty;
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
