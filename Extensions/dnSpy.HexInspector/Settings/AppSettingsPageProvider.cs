using System.Collections.Generic;
using System.ComponentModel.Composition;
using dnSpy.Contracts.Settings.Dialog;

namespace dnSpy.HexInspector.Settings {
	[Export(typeof(IAppSettingsPageProvider))]
	public class AppSettingsPageProvider : IAppSettingsPageProvider {
		readonly HexInspectorSettings settings;

		[ImportingConstructor]
		public AppSettingsPageProvider(HexInspectorSettings settings) => this.settings = settings;

		public IEnumerable<AppSettingsPage> Create() {
			yield return new HexInspectorSettingsPage(settings);
		}
	}
}
