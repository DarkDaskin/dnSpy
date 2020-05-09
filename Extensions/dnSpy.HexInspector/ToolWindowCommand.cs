using dnSpy.Contracts.Menus;

namespace dnSpy.HexInspector {
	[ExportMenuItem(OwnerGuid = MenuConstants.APP_MENU_VIEW_GUID, Header = "Hex inspector", InputGestureText = "Ctrl+Alt+X", Group = MenuConstants.GROUP_APP_MENU_VIEW_WINDOWS)]
	public class ToolWindowCommand : MenuItemCommand {
		public ToolWindowCommand() : base(ToolWindowLoader.OpenToolWindow) {
		}
	}
}
