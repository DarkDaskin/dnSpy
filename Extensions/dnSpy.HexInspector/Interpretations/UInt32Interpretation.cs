using System;
using System.Buffers.Binary;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	public class UInt32Interpretation : Interpretation {
		protected override int RequiredLength => sizeof(uint);
		public override string Name => "UInt32";

		public UInt32Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string ReadValue() =>
			(ByteOrder switch
			{
				ByteOrder.LittleEndian => Buffer!.ReadUInt32(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadUInt32BigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			}).ToString(CultureInfo.CurrentCulture);

		protected override bool TryWriteValue(string value) {
			if (uint.TryParse(value, out var rawValue)) {
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
