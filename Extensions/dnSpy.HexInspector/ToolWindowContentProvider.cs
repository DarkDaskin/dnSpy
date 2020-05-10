using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using dnSpy.Contracts.ToolWindows;
using dnSpy.Contracts.ToolWindows.App;

namespace dnSpy.HexInspector {
	[Export, Export(typeof(IToolWindowContentProvider))]
	public class ToolWindowContentProvider : IToolWindowContentProvider {
		readonly Lazy<HexInspectorViewModel> viewModel;
		HexInspectorToolWindowContent? content;

		public HexInspectorToolWindowContent Content => content ??= new HexInspectorToolWindowContent(viewModel.Value);

		[ImportingConstructor]
		public ToolWindowContentProvider(Lazy<HexInspectorViewModel> viewModel) => this.viewModel = viewModel;

		public ToolWindowContent? GetOrCreate(Guid guid) {
			if (guid == HexInspectorToolWindowContent.THE_GUID) {
				return Content;
			}
			return null;
		}

		public IEnumerable<ToolWindowContentInfo> ContentInfos {
			get {
				yield return new ToolWindowContentInfo(HexInspectorToolWindowContent.THE_GUID, HexInspectorToolWindowContent.DEFAULT_LOCATION);
			}
		}
	}
}
