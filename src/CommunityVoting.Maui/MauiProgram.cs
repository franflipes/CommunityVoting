using CommunityVoting.Maui.Services;
using CommunityVoting.Maui.ViewModels;
using CommunityVoting.Maui.Views;
using Microsoft.Extensions.Logging;

namespace CommunityVoting.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Services
		builder.Services.AddSingleton<AuthService>();
		builder.Services.AddSingleton<MeetingService>();
		builder.Services.AddSingleton<RealtimeVotingService>();

		// ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<MeetingsViewModel>();
		builder.Services.AddTransient<VotingViewModel>();

		// Views
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<MeetingsPage>();
		builder.Services.AddTransient<VotingPage>();

		return builder.Build();
	}
}
