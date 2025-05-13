using FootballLeague.ViewModels;

namespace FootballLeague.Views
{
    public partial class AddMatchPage : ContentPage
    {
        public AddMatchPage(AddMatchViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }



        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is AddMatchViewModel vm)
            {
                await vm.OnAppearing();
            }
        }
    }
}