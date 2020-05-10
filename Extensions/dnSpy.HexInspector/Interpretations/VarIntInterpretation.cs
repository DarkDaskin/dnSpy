using System.ComponentModel.Composition;
using System.Globalization;

namespace dnSpy.HexInspector.Interpretations
{
	[ExportInterpretation(InterpretationType.VarInt)]
	public class VarIntInterpretation : Interpretation {
		protected override int RequiredLength => 1;
		public override string Name => nameof(InterpretationType.VarInt);
		public override bool CanWrite => false;

		[ImportingConstructor]
		public VarIntInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string? ReadValue() {
			var rawValue = 0;
			var position = StartPosition;
			var endPosition = ParentViewModel.HexBufferSpan.End.Position;
			var bitOffset = 0;
			while (bitOffset < 35 && position < endPosition) {
				var b = Buffer!.ReadByte(position++);
				rawValue |= (b & 0x7F) << bitOffset;
				bitOffset += 7;
				if ((b & 0x80) == 0)
					return rawValue.ToString(CultureInfo.CurrentCulture);
			}
			return null;
		}

		protected override bool TryWriteValue(string value) => false;
	}
}
