using System;
using System.Buffers.Binary;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	public class DoubleInterpretation : Interpretation {
		protected override int RequiredLength => sizeof(double);
		public override string Name => "Double";

		public DoubleInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string ReadValue() =>
			(ByteOrder switch
			{
				ByteOrder.LittleEndian => Buffer!.ReadDouble(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadDoubleBigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			}).ToString(CultureInfo.CurrentCulture);

		protected override bool TryWriteValue(string value) {
			if (double.TryParse(value, out var doubleValue)) {
				var rawValue = BitConverter.DoubleToInt64Bits(doubleValue);
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
