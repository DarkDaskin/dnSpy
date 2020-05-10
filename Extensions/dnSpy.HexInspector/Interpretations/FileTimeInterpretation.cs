using System;
using System.Buffers.Binary;
using System.ComponentModel.Composition;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	[ExportInterpretation(InterpretationType.FileTime, DisplayName = DISPLAY_NAME)]
	public class FileTimeInterpretation : Interpretation {
		const string DISPLAY_NAME = "FILETIME";
		static readonly DateTime MIN_DATE = DateTime.FromFileTime(0);
		static readonly DateTime MAX_DATE = DateTime.MaxValue;
		static readonly long MIN_RAW = MIN_DATE.ToFileTime();
		static readonly long MAX_RAW = MAX_DATE.ToFileTime();

		protected override int RequiredLength => sizeof(long);
		public override string Name => nameof(InterpretationType.FileTime);
		public override string DisplayName => DISPLAY_NAME;

		[ImportingConstructor]
		public FileTimeInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string? ReadValue() {
			var rawValue = ByteOrder switch {
				ByteOrder.LittleEndian => Buffer!.ReadInt64(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadInt64BigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			};
			if (rawValue < MIN_RAW || rawValue > MAX_RAW) return null;
			return DateTime.FromFileTime(rawValue).ToString(CultureInfo.CurrentCulture);
		}

		protected override bool TryWriteValue(string value) {
			if (DateTime.TryParse(value, out var dateTimeValue) && dateTimeValue >= MIN_DATE && dateTimeValue <= MAX_DATE) {
				var rawValue = dateTimeValue.ToFileTime();
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
