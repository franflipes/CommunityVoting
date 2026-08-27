using CommunityVoting.Maui.ViewModels;

namespace CommunityVoting.Maui.Views;

public partial class VotingPage : ContentPage
{
    private readonly VotingViewModel _viewModel;

    public VotingPage(VotingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _viewModel.CleanupAsync();
    }
}
