using System;
using System.Buffers.Binary;
using System.ComponentModel.Composition;

namespace dnSpy.HexInspector.Interpretations
{
	[ExportInterpretation(InterpretationType.DosTime, DisplayName = DISPLAY_NAME)]
	public class DosTimeInterpretation : Interpretation {
		const string DISPLAY_NAME = "DOS time";
		internal static readonly TimeSpan MIN_TIME = TimeSpan.Zero;
		internal static readonly TimeSpan MAX_TIME = new TimeSpan(23, 59, 59);

		protected override int RequiredLength => sizeof(ushort);
		public override string Name => nameof(InterpretationType.DosTime);
		public override string DisplayName => DISPLAY_NAME;

		[ImportingConstructor]
		public DosTimeInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string? ReadValue() {
			var rawValue = ByteOrder switch {
				ByteOrder.LittleEndian => Buffer!.ReadUInt16(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadUInt16BigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			};
			return FromDosTime(rawValue)?.ToString();
		}

		internal static TimeSpan? FromDosTime(ushort rawValue) {
			var seconds = (rawValue & 0b1_1111) << 1;
			var minutes = (rawValue & 0b111_1110_0000) >> 5;
			var hours = rawValue >> 11;
			if (seconds > 59 || minutes > 59 || hours > 23) return null;
			return new TimeSpan(hours, minutes, seconds);
		}

		internal static ushort ToDosTime(TimeSpan time) =>
			(ushort)((time.Seconds >> 1) | (time.Minutes << 5) | (time.Hours << 11));

		protected override bool TryWriteValue(string value) {
			if (TimeSpan.TryParse(value, out var timeValue) && timeValue >= MIN_TIME && timeValue <= MAX_TIME) {
				var rawValue = ToDosTime(timeValue);
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