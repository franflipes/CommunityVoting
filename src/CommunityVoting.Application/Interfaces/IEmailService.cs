using CommunityVoting.Application.DTOs;

namespace CommunityVoting.Application.Interfaces;

public interface IEmailService
{
    Task SendMeetingInvitationAsync(
        string recipient,
        MeetingInvitationEmailModel model,
        CancellationToken cancellationToken = default);
}
