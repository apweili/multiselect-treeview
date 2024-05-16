using System.Collections.Generic;
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
            var selectedItems = value as List<object>;
            var displayMemberPath = parameter as string;
            if (selectedItems != null)
            {
                if (string.IsNullOrEmpty(displayMemberPath))
                {
                    return string.Join(";", selectedItems.Select(item => item.ToString()));
                }

                return string.Join(";",
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