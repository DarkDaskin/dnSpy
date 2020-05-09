using System;
using System.Buffers.Binary;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	public class Int16Interpretation : Interpretation {
		protected override int RequiredLength => sizeof(short);
		public override string Name => "Int16";

		public Int16Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string ReadValue() =>
			(ByteOrder switch
			{
				ByteOrder.LittleEndian => Buffer!.ReadInt16(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadInt16BigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			}).ToString(CultureInfo.CurrentCulture);

		protected override bool TryWriteValue(string value) {
			if (short.TryParse(value, out var rawValue)) {
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
