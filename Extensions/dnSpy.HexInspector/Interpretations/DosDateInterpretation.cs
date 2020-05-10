using System;
using System.Buffers.Binary;
using System.ComponentModel.Composition;

namespace dnSpy.HexInspector.Interpretations
{
	[ExportInterpretation(InterpretationType.DosDate, DisplayName = DISPLAY_NAME)]
	public class DosDateInterpretation : Interpretation {
		const string DISPLAY_NAME = "DOS date";
		const int EPOCH_YEAR = 1980;
		internal static readonly DateTime MIN_DATE = new DateTime(EPOCH_YEAR, 1, 1);
		internal static readonly DateTime MAX_DATE = new DateTime(EPOCH_YEAR + 0b111_1111, 12, 31);

		protected override int RequiredLength => sizeof(ushort);
		public override string Name => nameof(InterpretationType.DosDate);
		public override string DisplayName => DISPLAY_NAME;

		[ImportingConstructor]
		public DosDateInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string? ReadValue() {
			var rawValue = ByteOrder switch {
				ByteOrder.LittleEndian => Buffer!.ReadUInt16(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadUInt16BigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			};
			return FromDosDate(rawValue)?.ToShortDateString();
		}

		internal static DateTime? FromDosDate(ushort rawValue) {
			var day = rawValue & 0b1_1111;
			var month = (rawValue & 0b1_1110_0000) >> 5;
			var year = (rawValue >> 9) + EPOCH_YEAR;
			if (month < 1 || month > 12) return null;
			if (day < 1 || day > DateTime.DaysInMonth(year, month)) return null;
			return new DateTime(year, month, day);
		}

		internal static ushort ToDosDate(DateTime dateTime) =>
			(ushort) (dateTime.Day | (dateTime.Month << 5) | ((dateTime.Year - EPOCH_YEAR) << 9));

		protected override bool TryWriteValue(string value) {
			if (DateTime.TryParse(value, out var dateTimeValue) && dateTimeValue >= MIN_DATE && dateTimeValue <= MAX_DATE) {
				var rawValue = ToDosDate(dateTimeValue);
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