using Microsoft.AspNetCore.SignalR;

namespace CommunityVoting.Voting.Api.Hubs;

public class VotingHub : Hub
{
    public async Task JoinMeetingGroup(string meetingId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"meeting-{meetingId}");
    }

    public async Task LeaveMeetingGroup(string meetingId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"meeting-{meetingId}");
    }

    public async Task JoinSessionGroup(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");
    }

    public async Task LeaveSessionGroup(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session-{sessionId}");
    }
}
