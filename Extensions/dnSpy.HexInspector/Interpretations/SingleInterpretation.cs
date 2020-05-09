using System;
using System.Buffers.Binary;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	public class SingleInterpretation : Interpretation {
		protected override int RequiredLength => sizeof(float);
		public override string Name => "Single";

		public SingleInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string ReadValue() =>
			(ByteOrder switch
			{
				ByteOrder.LittleEndian => Buffer!.ReadSingle(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadSingleBigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			}).ToString(CultureInfo.CurrentCulture);

		protected override bool TryWriteValue(string value) {
			if (float.TryParse(value, out var floatValue)) {
#if NETCOREAPP3_1
					var rawValue = BitConverter.SingleToInt32Bits(floatValue);
#else
				var rawValue = BitConverter.ToInt32(BitConverter.GetBytes(floatValue), 0);
#endif
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
