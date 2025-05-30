// Plik: Views/ViewMatchesPage.xaml.cs
using FootballLeague.ViewModels;

namespace FootballLeague.Views
{
    public partial class ViewMatchesPage : ContentPage
    {
        public ViewMatchesPage(ViewMatchesViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }



        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ViewMatchesViewModel vm)
            {
                await vm.OnAppearing();
            }
        }
    }
}