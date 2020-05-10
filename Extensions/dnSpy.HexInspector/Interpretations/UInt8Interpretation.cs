using System.ComponentModel.Composition;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	[ExportInterpretation(InterpretationType.UInt8)]
	public class UInt8Interpretation : Interpretation {
		protected override int RequiredLength => sizeof(byte);
		public override string Name => nameof(InterpretationType.UInt8);

		[ImportingConstructor]
		public UInt8Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string ReadValue() =>
			Buffer!.ReadByte(StartPosition).ToString(CultureInfo.CurrentCulture);

		protected override bool TryWriteValue(string value) {
			if (byte.TryParse(value, out var rawValue)) {
				Buffer!.Replace(StartPosition, rawValue);
				return true;
			}
			return false;
		}
	}
}
