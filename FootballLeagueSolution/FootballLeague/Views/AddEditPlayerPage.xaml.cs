using FootballLeague.ViewModels;

namespace FootballLeague.Views
{
    public partial class AddEditPlayerPage : ContentPage
    {
        private AddEditPlayerViewModel? _viewModel;

        public AddEditPlayerPage(AddEditPlayerViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                await _viewModel.OnAppearing();
            }
        }
    }
}