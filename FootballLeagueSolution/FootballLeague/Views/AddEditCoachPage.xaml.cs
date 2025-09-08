using FootballLeague.ViewModels;
using System.Globalization;
using System;

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

    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue > 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}