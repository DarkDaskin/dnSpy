using System;
using dnSpy.Contracts.Settings.Dialog;

namespace dnSpy.HexInspector.Settings {
	public class HexInspectorSettingsPage : AppSettingsPage {
		static readonly Guid THE_GUID = new Guid("2CF38C7A-BCF2-444F-9BE7-9EC2D2147309");

		readonly HexInspectorSettings settings;
		readonly HexInspectorSettings editableSettings;
		readonly HexInspectorSettingsView view;

		public HexInspectorSettingsPage(HexInspectorSettings settings) {
			this.settings = settings;

			view = new HexInspectorSettingsView {DataContext = editableSettings = settings.Clone()};
		}

		public override Guid Guid => THE_GUID;
		public override double Order => AppSettingsConstants.ORDER_HEXEDITOR + 1;
		public override string Title => "Hex inspector";
		public override object? UIObject => view;
		public override void OnApply() => editableSettings.CopyTo(settings);
	}
}
