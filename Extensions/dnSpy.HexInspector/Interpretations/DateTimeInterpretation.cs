using System;
using System.Buffers.Binary;
using System.ComponentModel.Composition;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	[ExportInterpretation(InterpretationType.DateTime, DisplayName = DISPLAY_NAME)]
	public class DateTimeInterpretation : Interpretation {
		const string DISPLAY_NAME = "DateTime (.NET)";
		static readonly long MIN_RAW = DateTime.MinValue.ToBinary();
		static readonly long MAX_RAW = DateTime.MaxValue.ToBinary();

		protected override int RequiredLength => sizeof(long);
		public override string Name => nameof(InterpretationType.DateTime);
		public override string DisplayName => DISPLAY_NAME;

		[ImportingConstructor]
		public DateTimeInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string? ReadValue() {
			var rawValue = ByteOrder switch {
				ByteOrder.LittleEndian => Buffer!.ReadInt64(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadInt64BigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			};
			if (rawValue < MIN_RAW || rawValue > MAX_RAW) return null;
			return DateTime.FromBinary(rawValue).ToString(CultureInfo.CurrentCulture);
		}

		protected override bool TryWriteValue(string value) {
			if (DateTime.TryParse(value, out var dateTimeValue)) {
				var rawValue = dateTimeValue.ToBinary();
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
