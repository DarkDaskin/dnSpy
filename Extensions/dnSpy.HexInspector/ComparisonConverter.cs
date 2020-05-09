using System;
using System.Globalization;
using System.Windows.Data;

namespace dnSpy.HexInspector {
	public class ComparisonConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 
			value?.Equals(parameter) ?? false;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			value?.Equals(true) == true ? parameter : Binding.DoNothing;
	}
}
