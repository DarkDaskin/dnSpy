using System;
using System.Windows;
using dnSpy.Contracts.ToolWindows;
using dnSpy.Contracts.ToolWindows.App;

namespace dnSpy.HexInspector {
	public class HexInspectorToolWindowContent : ToolWindowContent {
		public static readonly Guid THE_GUID = new Guid("462BE21E-50A9-4EE6-891B-4182FE27CA29");
		public const AppToolWindowLocation DEFAULT_LOCATION = AppToolWindowLocation.DefaultHorizontal;

		readonly HexInspectorView view;

		public HexInspectorViewModel ViewModel { get; }

		public override object? UIObject => view;
		public override IInputElement? FocusedElement => null;
		public override FrameworkElement? ZoomElement => view;
		public override Guid Guid => THE_GUID;
		public override string Title => "Hex inspector";

		public HexInspectorToolWindowContent(HexInspectorViewModel viewModel) => 
			view = new HexInspectorView {DataContext = ViewModel = viewModel};
	}
}
