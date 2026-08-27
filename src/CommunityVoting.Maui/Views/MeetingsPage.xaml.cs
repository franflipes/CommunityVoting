using CommunityVoting.Maui.ViewModels;

namespace CommunityVoting.Maui.Views;

public partial class MeetingsPage : ContentPage
{
    private readonly MeetingsViewModel _viewModel;

    public MeetingsPage(MeetingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
