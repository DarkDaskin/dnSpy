using System;

namespace dnSpy.HexInspector.Interpretations
{
	public class StringInterpretation : Interpretation {
		const int MAX_CHARS = 50;
		static readonly char[] SPLIT_CHARS = {'\0', '\r', '\n', '�'};

		protected override int RequiredLength => 1;
		public override string Name => "String";
		public override bool CanWrite => false;

		public StringInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
		}

		protected override string ReadValue() {
			var length = Math.Min((ulong)ParentViewModel.Encoding.GetMaxByteCount(MAX_CHARS), ParentViewModel.HexBufferSpan.Length.ToUInt64());
			var bytes = Buffer!.ReadBytes(StartPosition, length);
			var str = ParentViewModel.Encoding.GetString(bytes);
			var splitIndex = str.IndexOfAny(SPLIT_CHARS);
			return splitIndex >= 0 ? str.Remove(splitIndex) : str;
		}

		protected override bool TryWriteValue(string value) => false;
	}
}
