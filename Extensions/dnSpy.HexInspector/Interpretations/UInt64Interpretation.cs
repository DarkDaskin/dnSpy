using System;
using System.Buffers.Binary;
using System.ComponentModel.Composition;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	[ExportInterpretation(InterpretationType.UInt64)]
	public class UInt64Interpretation : Interpretation {
		protected override int RequiredLength => sizeof(ulong);
		public override string Name => nameof(InterpretationType.UInt64);

		[ImportingConstructor]
		public UInt64Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string ReadValue() =>
			(ByteOrder switch {
				ByteOrder.LittleEndian => Buffer!.ReadUInt64(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadUInt64BigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			}).ToString(CultureInfo.CurrentCulture);

		protected override bool TryWriteValue(string value) {
			if (ulong.TryParse(value, out var rawValue)) {
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
