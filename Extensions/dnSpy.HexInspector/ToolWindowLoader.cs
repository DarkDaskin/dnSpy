using System.ComponentModel.Composition;
using System.Windows.Input;
using dnSpy.Contracts.Controls;
using dnSpy.Contracts.Extension;
using dnSpy.Contracts.MVVM;
using dnSpy.Contracts.ToolWindows.App;

namespace dnSpy.HexInspector {
	[ExportAutoLoaded]
	public class ToolWindowLoader : IAutoLoaded {
		public static readonly RoutedCommand OpenToolWindow = new RoutedCommand("OpenHexInspectorToolWindow", typeof(ToolWindowLoader));

		[ImportingConstructor]
		public ToolWindowLoader(IWpfCommandService wpfCommandService, IDsToolWindowService toolWindowService) {
			var commands = wpfCommandService.GetCommands(ControlConstants.GUID_MAINWINDOW);
			commands.Add(OpenToolWindow, new RelayCommand(a => toolWindowService.Show(HexInspectorToolWindowContent.THE_GUID)));
			commands.Add(OpenToolWindow, ModifierKeys.Control | ModifierKeys.Alt, Key.X);
		}
	}
}
