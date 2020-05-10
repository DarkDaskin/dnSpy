using System;
using System.Buffers.Binary;
using System.ComponentModel.Composition;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	[ExportInterpretation(InterpretationType.Timestamp32, DisplayName = DISPLAY_NAME)]
	public class Timestamp32Interpretation : Interpretation {
		const string DISPLAY_NAME = "time__t (32-bit)";
		internal static readonly DateTime EPOCH = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
		static readonly DateTime MIN_DATE = EPOCH.AddSeconds(int.MinValue);
		static readonly DateTime MAX_DATE = EPOCH.AddSeconds(int.MaxValue);

		protected override int RequiredLength => sizeof(int);
		public override string Name => nameof(InterpretationType.Timestamp32);
		public override string DisplayName => DISPLAY_NAME;

		[ImportingConstructor]
		public Timestamp32Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string? ReadValue() {
			var rawValue = ByteOrder switch {
				ByteOrder.LittleEndian => Buffer!.ReadInt32(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadInt32BigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			};
			return EPOCH.AddSeconds(rawValue).ToString(CultureInfo.CurrentCulture);
		}

		protected override bool TryWriteValue(string value) {
			if (DateTime.TryParse(value, out var dateTimeValue) && dateTimeValue >= MIN_DATE && dateTimeValue <= MAX_DATE) {
				var rawValue = (int)(dateTimeValue - EPOCH).TotalSeconds;
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
