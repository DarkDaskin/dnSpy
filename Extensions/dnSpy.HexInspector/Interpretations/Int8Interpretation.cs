using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	public class Int8Interpretation : Interpretation {
		protected override int RequiredLength => sizeof(sbyte);
		public override string Name => "Int8";

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
