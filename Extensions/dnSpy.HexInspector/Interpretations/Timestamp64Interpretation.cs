using System;
using System.Buffers.Binary;
using System.ComponentModel.Composition;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	[ExportInterpretation(InterpretationType.Timestamp64, DisplayName = DISPLAY_NAME)]
	public class Timestamp64Interpretation : Interpretation {
		const string DISPLAY_NAME = "time__t (64-bit)";
		static readonly DateTime EPOCH = Timestamp32Interpretation.EPOCH;
		static readonly long MIN_RAW = (long)(DateTime.MinValue - EPOCH).TotalSeconds;
		static readonly long MAX_RAW = (long)(DateTime.MaxValue - EPOCH).TotalSeconds - 1;

		protected override int RequiredLength => sizeof(long);
		public override string Name => nameof(InterpretationType.Timestamp64);
		public override string DisplayName => DISPLAY_NAME;

		[ImportingConstructor]
		public Timestamp64Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string? ReadValue() {
			var rawValue = ByteOrder switch {
				ByteOrder.LittleEndian => Buffer!.ReadInt64(StartPosition),
				ByteOrder.BigEndian => Buffer!.ReadInt64BigEndian(StartPosition),
				_ => throw new ArgumentOutOfRangeException()
			};
			if (rawValue < MIN_RAW || rawValue > MAX_RAW) return null;
			return EPOCH.AddSeconds(rawValue).ToString(CultureInfo.CurrentCulture);
		}

		protected override bool TryWriteValue(string value) {
			if (DateTime.TryParse(value, out var dateTimeValue)) {
				var rawValue = (long)(dateTimeValue - EPOCH).TotalSeconds;
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
