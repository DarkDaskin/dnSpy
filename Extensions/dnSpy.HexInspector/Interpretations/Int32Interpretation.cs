using System;
using System.Buffers.Binary;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	public class Int32Interpretation : Interpretation {
		protected override int RequiredLength => sizeof(int);
		public override string Name => "Int32";

		public Int32Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string ReadValue() =>
			(ByteOrder switch
			{
				ByteOrder.LittleEndian => Buffer!.ReadInt32(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadInt32BigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			}).ToString(CultureInfo.CurrentCulture);

		protected override bool TryWriteValue(string value) {
			if (int.TryParse(value, out var rawValue)) {
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
