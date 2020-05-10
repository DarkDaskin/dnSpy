using System.ComponentModel.Composition;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	[ExportInterpretation(InterpretationType.Int8)]
	public class Int8Interpretation : Interpretation {
		protected override int RequiredLength => sizeof(sbyte);
		public override string Name => nameof(InterpretationType.Int8);

		[ImportingConstructor]
		public Int8Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string ReadValue() =>
			Buffer!.ReadSByte(StartPosition).ToString(CultureInfo.CurrentCulture);

		protected override bool TryWriteValue(string value) {
			if (sbyte.TryParse(value, out var rawValue)) {
				Buffer!.Replace(StartPosition, rawValue);
				return true;
			}
			return false;
		}
	}
}
