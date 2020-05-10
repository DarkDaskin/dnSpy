using System;
using System.ComponentModel.Composition;

namespace dnSpy.HexInspector.Interpretations
{
	[ExportInterpretation(InterpretationType.Guid, DisplayName = DISPLAY_NAME)]
	public class GuidInterpretation : Interpretation {
		const string DISPLAY_NAME = "GUID";

		protected override int RequiredLength => 16;
		public override string Name => nameof(InterpretationType.Guid);
		public override string DisplayName => DISPLAY_NAME;

		[ImportingConstructor]
		public GuidInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string ReadValue() {
			var bytes = Buffer!.ReadBytes(StartPosition, RequiredLength);
			if (NeedByteOrderSwap) {
				Array.Reverse(bytes);
			}
			return new Guid(bytes).ToString();
		}

		protected override bool TryWriteValue(string value) {
			if (Guid.TryParse(value, out var guidValue)) {
				var bytes = guidValue.ToByteArray();
				if (NeedByteOrderSwap) {
					Array.Reverse(bytes);
				}
				Buffer!.Replace(StartPosition, bytes);
				return true;
			}
			return false;
		}
	}
}
