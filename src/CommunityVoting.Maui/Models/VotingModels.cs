namespace CommunityVoting.Maui.Models;

public class VotingSessionModel
{
    public string Id { get; set; } = string.Empty;
    public string MeetingId { get; set; } = string.Empty;
    public string ProposalId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<VotingOptionModel> Options { get; set; } = new();
}

public class VotingOptionModel
{
    public string OptionId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Votes { get; set; }
}

public record CastVoteRequest(string SessionId, string OptionId, double Coefficient = 1.0);
public record VoteCastNotification(string SessionId, string OptionId, int TotalVotesForOption);
