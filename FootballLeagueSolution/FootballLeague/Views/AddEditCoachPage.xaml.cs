using FootballLeague.ViewModels;

namespace FootballLeague.Views
{
    public partial class AddEditCoachPage : ContentPage
    {
        public AddEditCoachPage(AddEditCoachViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}