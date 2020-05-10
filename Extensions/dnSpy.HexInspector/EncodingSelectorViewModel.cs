using System;
using System.Linq;
using System.Text;
using dnSpy.Contracts.MVVM;

namespace dnSpy.HexInspector {
	public class EncodingSelectorViewModel : ViewModelBase {
		static readonly EncodingInfo[] ENCODINGS;

		int selectedIndex = 0;

		public EncodingInfo[] Encodings => ENCODINGS;

		public int SelectedIndex {
			get => selectedIndex;
			set {
				if (selectedIndex != value) {
					selectedIndex = value;
					Encoding = Encodings[value].GetEncoding();
					OnPropertyChanged(nameof(SelectedIndex));
					OnPropertyChanged(nameof(Encoding));
				}
			}
		}

		public Encoding Encoding { get; private set; }

		static EncodingSelectorViewModel() {
			var preferredCodePages = new[] {Encoding.Default.CodePage, Encoding.Unicode.CodePage, Encoding.UTF8.CodePage};
			ENCODINGS = Encoding.GetEncodings()
				.OrderByDescending(encoding => Array.IndexOf(preferredCodePages, encoding.CodePage))
				.ThenBy(encoding => encoding.DisplayName, StringComparer.CurrentCultureIgnoreCase)
				.ToArray();
		}

		public EncodingSelectorViewModel(int? selectedCodePage = null) {
			EncodingInfo? selectedEncoding = null;
			if (selectedCodePage.HasValue)
				selectedEncoding = ENCODINGS.FirstOrDefault(encoding => encoding.CodePage == selectedCodePage);
			selectedEncoding ??= ENCODINGS.First();
			Encoding = selectedEncoding.GetEncoding();
			selectedIndex = Array.IndexOf(ENCODINGS, selectedEncoding);
		}
	}
}
