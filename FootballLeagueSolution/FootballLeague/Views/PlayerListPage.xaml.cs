using FootballLeague.ViewModels;

namespace FootballLeague.Views 
{
    public partial class PlayerListPage : ContentPage
    {
        public PlayerListPage(PlayerListViewModel viewModel) 
        {
            InitializeComponent(); 
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is PlayerListViewModel vm)
            {
                await vm.OnAppearing();
            }
        }
    }
}