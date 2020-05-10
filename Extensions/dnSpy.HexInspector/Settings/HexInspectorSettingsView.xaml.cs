using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace dnSpy.HexInspector.Settings {
	public partial class HexInspectorSettingsView : UserControl {
		public HexInspectorSettingsView() => InitializeComponent();
		
		void Grid_Loaded(object sender, RoutedEventArgs e) {
			// Work-around for broken RelativeSource.
			var target = (DependencyObject)sender;
			var binding = BindingOperations.GetBinding(target, FrameworkElement.HeightProperty);
			BindingOperations.SetBinding(target, FrameworkElement.HeightProperty, binding);
		}
	}
}
