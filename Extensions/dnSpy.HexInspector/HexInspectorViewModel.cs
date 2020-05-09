using System;
using System.Linq;
using System.Text;
using dnSpy.Contracts.Hex;
using dnSpy.Contracts.MVVM;
using dnSpy.HexInspector.Interpretations;
using Microsoft.VisualStudio.Language.Intellisense;

namespace dnSpy.HexInspector {
	public class HexInspectorViewModel : ViewModelBase {
		HexBufferSpan hexBufferSpan;
		ByteOrder byteOrder;
		int encodingIndex = 0;

		public BulkObservableCollection<Interpretation> Interpretations { get; } = new BulkObservableCollection<Interpretation>();
		public BulkObservableCollection<EncodingInfo> Encodings { get; } = new BulkObservableCollection<EncodingInfo>();

		public HexBufferSpan HexBufferSpan {
			get => hexBufferSpan;
			set {
				if (hexBufferSpan != value) {
					hexBufferSpan = value;
					OnPropertyChanged(nameof(HexBufferSpan));
				}
			}
		}

		public ByteOrder ByteOrder {
			get => byteOrder;
			set {
				if (byteOrder != value) {
					byteOrder = value;
					OnPropertyChanged(nameof(ByteOrder));
				}
			}
		}

		public int EncodingIndex {
			get => encodingIndex;
			set {
				if (encodingIndex != value) {
					encodingIndex = value;
					Encoding = Encodings[value].GetEncoding();
					OnPropertyChanged(nameof(EncodingIndex));
					OnPropertyChanged(nameof(Encoding));
				}
			}
		}

		internal Encoding Encoding { get; private set; }

		public HexInspectorViewModel() {
			Interpretations.AddRange(new Interpretation[] {
				new UInt8Interpretation(this), 
				new Int8Interpretation(this), 
				new UInt16Interpretation(this), 
				new Int16Interpretation(this),
				new UInt32Interpretation(this), 
				new Int32Interpretation(this), 
				new UInt64Interpretation(this),
				new Int64Interpretation(this), 
				new SingleInterpretation(this), 
				new DoubleInterpretation(this),
				new BinaryInterpretation(this),
				new GuidInterpretation(this),
				new VarIntInterpretation(this),
				new StringInterpretation(this),
			});

			var preferredCodePages = new[] {
				Encoding.Default.CodePage,
				Encoding.Unicode.CodePage,
				Encoding.UTF8.CodePage,
			};
			var encodings = Encoding.GetEncodings().OrderByDescending(encoding => Array.IndexOf(preferredCodePages, encoding.CodePage));
			Encodings.AddRange(encodings);
			Encoding = Encodings[encodingIndex].GetEncoding();
		}

		public void OnBufferChanged() => OnPropertyChanged(nameof(HexBufferSpan));
	}
}
