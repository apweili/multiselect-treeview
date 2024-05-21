using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Helpers;

namespace System.Windows.Converters
{
    public class SelectionBoxItemValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const string splitSymbol = "；";
            var selectedItems = (value as IEnumerable)?.OfType<object>();
            var displayMemberPath = parameter as string;
            if (selectedItems != null)
            {
                if (string.IsNullOrEmpty(displayMemberPath))
                {
                    return string.Join(splitSymbol, selectedItems.Select(item => item.ToString()));
                }

                return string.Join(splitSymbol,
                    selectedItems.Select(item => PropertyPathHelper.GetObjectByPropertyPath(item, displayMemberPath)));
            }

            if (value == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(displayMemberPath))
            {
                return value.ToString();
            }

            return PropertyPathHelper.GetObjectByPropertyPath(value, displayMemberPath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}