using System;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;

namespace dnSpy.HexInspector.Interpretations
{
	[ExportInterpretation(InterpretationType.Binary, DisplayName = DISPLAY_NAME)]
	public class BinaryInterpretation : Interpretation {
		const string DISPLAY_NAME = "Binary (8-bit)";

		protected override int RequiredLength => sizeof(byte);
		public override string Name => nameof(InterpretationType.Binary);
		public override string DisplayName => DISPLAY_NAME;

		[ImportingConstructor]
		public BinaryInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string ReadValue() {
			var rawValue = Buffer!.ReadByte(StartPosition);
#if NETCOREAPP3_1
			Span<char> chars = stackalloc char[8];
#else
			var chars = new char[8];
#endif
			for (var i = 0; i < chars.Length; i++) {
				chars[chars.Length - 1 - i] = (rawValue & (1 << i)) != 0 ? '1' : '0';
			}
			return new string(chars);
		}

		protected override bool TryWriteValue(string value) {
			if (Regex.IsMatch(value, @"^[01]{1,8}$")) {
				byte rawValue = 0;
				for (var i = 0; i < value.Length; i++) {
					rawValue |= (byte)(value[value.Length - 1 - i] == '1' ? 1 << i : 0);
				}
				Buffer!.Replace(StartPosition, rawValue);
				return true;
			}
			return false;
		}
	}
}
