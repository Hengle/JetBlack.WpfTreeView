using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JetBlack.WpfTreeView.Converters
{
    public class NullableBoolConverter : IValueConverter
    {
        public object True { get; set; }
        public object False { get; set; }
        public object Null { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var nullableBool = value as bool?;

            if (nullableBool == null)
                return Null;
            if (nullableBool == true)
                return True;
            return False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == True)
                return true;
            if (value == False)
                return false;
            if (value == Null)
                return null;

            return DependencyProperty.UnsetValue;
        }
    }
}
