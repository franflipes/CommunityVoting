using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityVoting.Maui.Models;
using CommunityVoting.Maui.Services;

namespace CommunityVoting.Maui.ViewModels;

[QueryProperty(nameof(MeetingId), "meetingId")]
[QueryProperty(nameof(MeetingTitle), "title")]
public partial class VotingViewModel : ObservableObject
{
    private readonly RealtimeVotingService _votingService;
    private readonly MeetingService _meetingService;

    [ObservableProperty]
    private string _meetingId = string.Empty;

    [ObservableProperty]
    private string _meetingTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ProposalDto> _proposals = new();

    [ObservableProperty]
    private ProposalDto? _selectedProposal;

    partial void OnSelectedProposalChanged(ProposalDto? value)
    {
        SelectedOption = null;
    }

    [ObservableProperty]
    private ProposalOptionDto? _selectedOption;

    [RelayCommand]
    private void SelectOption(ProposalOptionDto option)
    {
        SelectedOption = option;
    }

    [ObservableProperty]
    private string _statusMessage = "Conectando a sesión en tiempo real...";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasVoted;

    public VotingViewModel(RealtimeVotingService votingService, MeetingService meetingService)
    {
        _votingService = votingService;
        _meetingService = meetingService;

        _votingService.SessionOpened += OnSessionOpened;
        _votingService.VoteCastReceived += OnVoteCast;
        _votingService.SessionClosed += OnSessionClosed;
    }

    public async Task InitializeAsync()
    {
        if (Guid.TryParse(MeetingId, out var gId))
        {
            var details = await _meetingService.GetMeetingDetailsAsync(gId);
            if (details != null && details.Proposals != null)
            {
                Proposals.Clear();
                foreach (var p in details.Proposals)
                {
                    Proposals.Add(p);
                }
                if (Proposals.Any())
                {
                    SelectedProposal = Proposals.First();
                }
            }
        }

        StatusMessage = "Conectando con SignalR...";
        await _votingService.InitializeAsync();
        if (!string.IsNullOrEmpty(MeetingId))
        {
            await _votingService.JoinMeetingGroupAsync(MeetingId);
            StatusMessage = "En línea. Esperando votación activa.";
        }
    }

    private void OnSessionOpened(VotingSessionModel session)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusMessage = $"¡Votación abierta!: {session.Title}";
            HasVoted = false;
        });
    }

    private void OnVoteCast(VoteCastNotification notification)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusMessage = $"Voto registrado en tiempo real en la opción {notification.OptionId}.";
        });
    }

    private void OnSessionClosed(string sessionId)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusMessage = "La votación ha sido cerrada.";
        });
    }

    [RelayCommand]
    private async Task SubmitVoteAsync()
    {
        if (SelectedProposal == null || SelectedOption == null)
        {
            StatusMessage = "Por favor selecciona una propuesta y una opción.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Enviando voto...";

            bool success = await _votingService.SubmitVoteAsync(SelectedProposal.Id.ToString(), SelectedOption.Id.ToString());
            if (success)
            {
                HasVoted = true;
                StatusMessage = "¡Voto emitido con éxito!";
            }
            else
            {
                StatusMessage = "No se pudo emitir el voto (puede que la sesión no esté abierta).";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task CleanupAsync()
    {
        _votingService.SessionOpened -= OnSessionOpened;
        _votingService.VoteCastReceived -= OnVoteCast;
        _votingService.SessionClosed -= OnSessionClosed;
        await _votingService.DisconnectAsync();
    }
}
