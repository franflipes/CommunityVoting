using CommunityVoting.Maui.Views;

namespace CommunityVoting.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("VotingPage", typeof(VotingPage));
	}
}
