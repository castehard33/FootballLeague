using System.Globalization;
using System.Collections; 

namespace FootballLeague.Converters
{
    public class ItemIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null || parameter is not IEnumerable itemsSource)
                return 0;

            var item = value;
            int index = 0;
            foreach (var listItem in itemsSource)
            {
                if (listItem == item)
                {
                    return index + 1; 
                }
                index++;
            }
            return 0; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}