using System;
using System.Buffers.Binary;
using System.ComponentModel.Composition;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	[ExportInterpretation(InterpretationType.UInt16)]
	public class UInt16Interpretation : Interpretation {
		protected override int RequiredLength => sizeof(ushort);
		public override string Name => nameof(InterpretationType.UInt16);

		[ImportingConstructor]
		public UInt16Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string ReadValue() =>
			(ByteOrder switch {
				ByteOrder.LittleEndian => Buffer!.ReadUInt16(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadUInt16BigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			}).ToString(CultureInfo.CurrentCulture);

		protected override bool TryWriteValue(string value) {
			if (ushort.TryParse(value, out var rawValue)) {
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
