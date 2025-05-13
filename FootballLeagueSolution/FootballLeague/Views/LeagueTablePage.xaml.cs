// Views/LeagueTablePage.xaml.cs
using FootballLeague.ViewModels;

namespace FootballLeague.Views
{
    public partial class LeagueTablePage : ContentPage
    {
        public LeagueTablePage(LeagueTableViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is LeagueTableViewModel vm)
            {
                await vm.OnAppearing();
            }
        }
    }
}