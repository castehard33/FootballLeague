using FootballLeague.ViewModels;

namespace FootballLeague.Views
{
    public partial class CoachListPage : ContentPage
    {
        public CoachListPage(CoachListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is CoachListViewModel vm)
            {
                await vm.OnAppearing();
            }
        }
    }
}