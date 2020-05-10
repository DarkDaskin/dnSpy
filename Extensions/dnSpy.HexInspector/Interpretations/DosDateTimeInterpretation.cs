using System;
using System.Buffers.Binary;
using System.ComponentModel.Composition;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	[ExportInterpretation(InterpretationType.DosDateTime, DisplayName = DISPLAY_NAME)]
	public class DosDateTimeInterpretation : Interpretation {
		const string DISPLAY_NAME = "DOS date & time";
		static readonly DateTime MIN_DATE = DosDateInterpretation.MIN_DATE;
		static readonly DateTime MAX_DATE = DosDateInterpretation.MAX_DATE + DosTimeInterpretation.MAX_TIME;

		protected override int RequiredLength => sizeof(uint);
		public override string Name => nameof(InterpretationType.DosDateTime);
		public override string DisplayName => DISPLAY_NAME;

		[ImportingConstructor]
		public DosDateTimeInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string? ReadValue() {
			var rawValue = ByteOrder switch {
				ByteOrder.LittleEndian => Buffer!.ReadUInt32(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadUInt32BigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			};
			var rawTime = (ushort)(rawValue & 0xFFFF);
			var rawDate = (ushort)((rawValue & 0xFFFF0000) >> 16);
			return (DosDateInterpretation.FromDosDate(rawDate) + DosTimeInterpretation.FromDosTime(rawTime))?.ToString(CultureInfo.CurrentCulture);
		}

		protected override bool TryWriteValue(string value) {
			if (DateTime.TryParse(value, out var dateTimeValue) && dateTimeValue >= MIN_DATE && dateTimeValue <= MAX_DATE) {
				var rawDate = DosDateInterpretation.ToDosDate(dateTimeValue.Date);
				var rawTime = DosTimeInterpretation.ToDosTime(dateTimeValue.TimeOfDay);
				var rawValue = rawTime | (rawDate << 16);
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