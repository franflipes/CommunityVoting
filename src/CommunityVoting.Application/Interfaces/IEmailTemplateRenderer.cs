using CommunityVoting.Application.DTOs;

namespace CommunityVoting.Application.Interfaces;

public interface IEmailTemplateRenderer
{
    Task<(string html, string text)> RenderMeetingInvitationAsync(MeetingInvitationEmailModel model);
}
