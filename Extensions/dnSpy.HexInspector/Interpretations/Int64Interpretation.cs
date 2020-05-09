using System;
using System.Buffers.Binary;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	public class Int64Interpretation : Interpretation {
		protected override int RequiredLength => sizeof(long);
		public override string Name => "Int64";

		public Int64Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string ReadValue() =>
			(ByteOrder switch
			{
				ByteOrder.LittleEndian => Buffer!.ReadInt64(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadInt64BigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			}).ToString(CultureInfo.CurrentCulture);

		protected override bool TryWriteValue(string value) {
			if (long.TryParse(value, NumberStyles.Integer | NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var rawValue)) {
				if (NeedByteOrderSwap) {
					rawValue = BinaryPrimitives.ReverseEndianness(rawValue);
				}
				Buffer!.Replace(StartPosition, rawValue);
				return true;
			}
			return false;
		}
	}
}
