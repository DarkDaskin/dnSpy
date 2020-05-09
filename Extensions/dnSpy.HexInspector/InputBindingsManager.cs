using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace dnSpy.HexInspector {
	public static class InputBindingsManager {
		public static readonly DependencyProperty CommitOnEnterProperty = DependencyProperty.RegisterAttached(
			"CommitOnEnter", typeof(DependencyProperty), typeof(InputBindingsManager), new PropertyMetadata(null, OnCommitOnEnterChanged));

		public static void SetCommitOnEnter(DependencyObject element, DependencyProperty value) => element.SetValue(CommitOnEnterProperty, value);

		public static DependencyProperty? GetCommitOnEnter(DependencyObject element) => (DependencyProperty)element.GetValue(CommitOnEnterProperty);

		static void OnCommitOnEnterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (!(d is UIElement element)) return;

			if (e.OldValue != null) {
				element.PreviewKeyDown -= OnPreviewKeyDown;
			}

			if (e.NewValue != null) {
				element.PreviewKeyDown += OnPreviewKeyDown;
			}
		}

		static void OnPreviewKeyDown(object sender, KeyEventArgs e) {
			if ((e.Key == Key.Enter || e.Key == Key.Return) && e.Source is DependencyObject target) {
				UpdateSource(target, GetCommitOnEnter(target));
			}
		}

		static void UpdateSource(DependencyObject target, DependencyProperty? property) {
			if (property == null) return;

			var bindingExpression = BindingOperations.GetBindingExpressionBase(target, property);
			bindingExpression?.UpdateSource();
		}
	}
}
