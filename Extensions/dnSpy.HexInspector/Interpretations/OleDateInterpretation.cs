using System;
using System.Buffers.Binary;
using System.ComponentModel.Composition;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations {
	[ExportInterpretation(InterpretationType.OleDate, DisplayName = DISPLAY_NAME)]
	public class OleDateInterpretation : Interpretation {
		const string DISPLAY_NAME = "OLE date";
		static readonly DateTime MIN_DATE = new DateTime(100, 1, 1);
		static readonly DateTime MAX_DATE = DateTime.MaxValue;
		static readonly double MIN_RAW = MIN_DATE.ToOADate();
		static readonly double MAX_RAW = MAX_DATE.ToOADate();

		protected override int RequiredLength => sizeof(double);
		public override string Name => nameof(InterpretationType.OleDate);
		public override string DisplayName => DISPLAY_NAME;

		[ImportingConstructor]
		public OleDateInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string? ReadValue() {
			var rawValue = ByteOrder switch {
				ByteOrder.LittleEndian => Buffer!.ReadDouble(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadDoubleBigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			};
			if (rawValue < MIN_RAW || rawValue > MAX_RAW) return null;
			return DateTime.FromOADate(rawValue).ToString(CultureInfo.CurrentCulture);
		}

		protected override bool TryWriteValue(string value) {
			if (DateTime.TryParse(value, out var dateTimeValue) && dateTimeValue >= MIN_DATE && dateTimeValue <= MAX_DATE) {
				var rawValue = BitConverter.DoubleToInt64Bits(dateTimeValue.ToOADate());
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
