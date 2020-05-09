using System;

namespace dnSpy.HexInspector.Interpretations
{
	public class GuidInterpretation : Interpretation {
		protected override int RequiredLength => 16;
		public override string Name => "GUID";

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
