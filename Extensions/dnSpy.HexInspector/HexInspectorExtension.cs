using System;
using System.Collections.Generic;
using dnSpy.Contracts.Extension;

namespace dnSpy.HexInspector
{
	[ExportExtension]
	public class HexInspectorExtension : IExtension
    {
	    public void OnEvent(ExtensionEvent @event, object? obj) {
	    }

	    public ExtensionInfo ExtensionInfo => new ExtensionInfo {
			ShortDescription = "Hex editor inspector",
	    };

	    public IEnumerable<string> MergedResourceDictionaries => Array.Empty<string>();
    }
}
