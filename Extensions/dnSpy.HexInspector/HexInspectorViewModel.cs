using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using dnSpy.Contracts.Hex;
using dnSpy.Contracts.MVVM;
using Microsoft.VisualStudio.Language.Intellisense;

namespace dnSpy.HexInspector {
	public class HexInspectorViewModel : ViewModelBase {
		HexBufferSpan hexBufferSpan;
		ByteOrder byteOrder;
		int encodingIndex = 0;
		Encoding encoding;

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
					encoding = Encodings[value].GetEncoding();
					OnPropertyChanged(nameof(EncodingIndex));
				}
			}
		}

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
			encoding = Encodings[encodingIndex].GetEncoding();
		}

		public void OnBufferChanged() => OnPropertyChanged(nameof(HexBufferSpan));
		

		public abstract class Interpretation : ViewModelBase {
			string? value;
			bool hasError;

			protected HexInspectorViewModel ParentViewModel { get; }

			protected abstract int RequiredLength { get; }
			public abstract string Name { get; }


			public string? Value {
				get => value;
				set {
					if (this.value != value) {
						this.value = value;
						OnPropertyChanged(nameof(Value));
						OnPropertyChanged(nameof(IsValid));

						hasError = value != null ? !TryWriteValue(value) : CanWrite;
						HasErrorUpdated();
					}
				}
			}

			public bool IsAvailable => ParentViewModel.HexBufferSpan.Length >= RequiredLength;
			public virtual bool CanWrite => Buffer != null && !Buffer.IsReadOnly && IsAvailable;
			public bool IsReadOnly => !CanWrite;
			public bool IsValid => value != null;

			protected HexBuffer? Buffer => ParentViewModel.HexBufferSpan.Buffer;
			protected HexPosition StartPosition => ParentViewModel.HexBufferSpan.Start.Position;
			protected ByteOrder ByteOrder => ParentViewModel.ByteOrder;
			protected bool NeedByteOrderSwap => BitConverter.IsLittleEndian != (ByteOrder == ByteOrder.LittleEndian);

			protected Interpretation(HexInspectorViewModel parentViewModel) {
				ParentViewModel = parentViewModel;
				ParentViewModel.PropertyChanged += OnParentViewModelPropertyChanged;
			}

			protected abstract string? ReadValue();
			protected abstract bool TryWriteValue(string value);

			void OnParentViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
				if (e.PropertyName == nameof(HexBufferSpan)) {
					OnPropertyChanged(nameof(IsAvailable));
					OnPropertyChanged(nameof(CanWrite));
				}

				var dependsOnByteOrder = RequiredLength > 1;
				var dependsOnEncoding = this is StringInterpretation;
				if (e.PropertyName == nameof(HexBufferSpan) || dependsOnByteOrder && e.PropertyName == nameof(ByteOrder) || dependsOnEncoding && e.PropertyName == nameof(EncodingIndex)) {
					value = IsAvailable ? ReadValue() : null;
					OnPropertyChanged(nameof(Value));
					OnPropertyChanged(nameof(IsValid));
					hasError = false;
					HasErrorUpdated();
				}
			}

			public override bool HasError => hasError;

			protected override string? Verify(string columnName) {
				if (columnName == nameof(Value) && hasError) {
					return "Invalid value";
				}
				return base.Verify(columnName);
			}
		}

		public class UInt8Interpretation : Interpretation {
			protected override int RequiredLength => sizeof(byte);
			public override string Name => "UInt8";

			public UInt8Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
			}

			protected override string ReadValue() => 
				Buffer!.ReadByte(StartPosition).ToString(CultureInfo.CurrentCulture);

			protected override bool TryWriteValue(string value) {
				if (byte.TryParse(value, out var rawValue)) {
					Buffer!.Replace(StartPosition, rawValue);
					return true;
				}
				return false;
			}
		}

		public class Int8Interpretation : Interpretation {
			protected override int RequiredLength => sizeof(sbyte);
			public override string Name => "Int8";

			public Int8Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
			}

			protected override string ReadValue() => 
				Buffer!.ReadSByte(StartPosition).ToString(CultureInfo.CurrentCulture);

			protected override bool TryWriteValue(string value) {
				if (sbyte.TryParse(value, out var rawValue)) {
					Buffer!.Replace(StartPosition, rawValue);
					return true;
				}
				return false;
			}
		}

		public class UInt16Interpretation : Interpretation {
			protected override int RequiredLength => sizeof(ushort);
			public override string Name => "UInt16";

			public UInt16Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
			}

			protected override string ReadValue() =>
				(ByteOrder switch {
					ByteOrder.LittleEndian => Buffer!.ReadUInt16(StartPosition),
					ByteOrder.BigEndian => Buffer!.ReadUInt16BigEndian(StartPosition),
					_ => throw new ArgumentOutOfRangeException()
				}).ToString(CultureInfo.CurrentCulture);

			protected override bool TryWriteValue(string value) {
				if (ushort.TryParse(value, out var rawValue)) {
					if (NeedByteOrderSwap) {
						rawValue = BinaryPrimitives.ReverseEndianness(rawValue);
					}
					Buffer!.Replace(StartPosition, rawValue);
					return true;
				}
				return false;
			}
		}

		public class Int16Interpretation : Interpretation {
			protected override int RequiredLength => sizeof(short);
			public override string Name => "Int16";

			public Int16Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
			}

			protected override string ReadValue() =>
				(ByteOrder switch {
					ByteOrder.LittleEndian => Buffer!.ReadInt16(StartPosition),
					ByteOrder.BigEndian => Buffer!.ReadInt16BigEndian(StartPosition),
					_ => throw new ArgumentOutOfRangeException()
				}).ToString(CultureInfo.CurrentCulture);

			protected override bool TryWriteValue(string value) {
				if (short.TryParse(value, out var rawValue)) {
					if (NeedByteOrderSwap) {
						rawValue = BinaryPrimitives.ReverseEndianness(rawValue);
					}
					Buffer!.Replace(StartPosition, rawValue);
					return true;
				}
				return false;
			}
		}

		public class UInt32Interpretation : Interpretation {
			protected override int RequiredLength => sizeof(uint);
			public override string Name => "UInt32";

			public UInt32Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
			}

			protected override string ReadValue() =>
				(ByteOrder switch {
					ByteOrder.LittleEndian => Buffer!.ReadUInt32(StartPosition),
					ByteOrder.BigEndian => Buffer!.ReadUInt32BigEndian(StartPosition),
					_ => throw new ArgumentOutOfRangeException()
				}).ToString(CultureInfo.CurrentCulture);

			protected override bool TryWriteValue(string value) {
				if (uint.TryParse(value, out var rawValue)) {
					if (NeedByteOrderSwap) {
						rawValue = BinaryPrimitives.ReverseEndianness(rawValue);
					}
					Buffer!.Replace(StartPosition, rawValue);
					return true;
				}
				return false;
			}
		}

		public class Int32Interpretation : Interpretation {
			protected override int RequiredLength => sizeof(int);
			public override string Name => "Int32";

			public Int32Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
			}

			protected override string ReadValue() =>
				(ByteOrder switch {
					ByteOrder.LittleEndian => Buffer!.ReadInt32(StartPosition),
					ByteOrder.BigEndian => Buffer!.ReadInt32BigEndian(StartPosition),
					_ => throw new ArgumentOutOfRangeException()
				}).ToString(CultureInfo.CurrentCulture);

			protected override bool TryWriteValue(string value) {
				if (int.TryParse(value, out var rawValue)) {
					if (NeedByteOrderSwap) {
						rawValue = BinaryPrimitives.ReverseEndianness(rawValue);
					}
					Buffer!.Replace(StartPosition, rawValue);
					return true;
				}
				return false;
			}
		}

		public class UInt64Interpretation : Interpretation {
			protected override int RequiredLength => sizeof(ulong);
			public override string Name => "UInt64";

			public UInt64Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
			}

			protected override string ReadValue() =>
				(ByteOrder switch {
					ByteOrder.LittleEndian => Buffer!.ReadUInt64(StartPosition),
					ByteOrder.BigEndian => Buffer!.ReadUInt64BigEndian(StartPosition),
					_ => throw new ArgumentOutOfRangeException()
				}).ToString(CultureInfo.CurrentCulture);

			protected override bool TryWriteValue(string value) {
				if (ulong.TryParse(value, out var rawValue)) {
					if (NeedByteOrderSwap) {
						rawValue = BinaryPrimitives.ReverseEndianness(rawValue);
					}
					Buffer!.Replace(StartPosition, rawValue);
					return true;
				}
				return false;
			}
		}

		public class Int64Interpretation : Interpretation {
			protected override int RequiredLength => sizeof(long);
			public override string Name => "Int64";

			public Int64Interpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
			}

			protected override string ReadValue() =>
				(ByteOrder switch {
					ByteOrder.LittleEndian => Buffer!.ReadInt64(StartPosition),
					ByteOrder.BigEndian => Buffer!.ReadInt64BigEndian(StartPosition),
					_ => throw new ArgumentOutOfRangeException()
				}).ToString(CultureInfo.CurrentCulture);

			protected override bool TryWriteValue(string value) {
				if (long.TryParse(value, NumberStyles.Integer | NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var rawValue)) {
					if (NeedByteOrderSwap) {
						rawValue = BinaryPrimitives.ReverseEndianness(rawValue);
					}
					Buffer!.Replace(StartPosition, rawValue);
					return true;
				}
				return false;
			}
		}

		public class SingleInterpretation : Interpretation {
			protected override int RequiredLength => sizeof(float);
			public override string Name => "Single";

			public SingleInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
			}

			protected override string ReadValue() =>
				(ByteOrder switch {
					ByteOrder.LittleEndian => Buffer!.ReadSingle(StartPosition),
					ByteOrder.BigEndian => Buffer!.ReadSingleBigEndian(StartPosition),
					_ => throw new ArgumentOutOfRangeException()
				}).ToString(CultureInfo.CurrentCulture);

			protected override bool TryWriteValue(string value) {
				if (float.TryParse(value, out var floatValue)) {
#if NETCOREAPP3_1
					var rawValue = BitConverter.SingleToInt32Bits(floatValue);
#else
					var rawValue = BitConverter.ToInt32(BitConverter.GetBytes(floatValue), 0);
#endif
					if (NeedByteOrderSwap) {
						rawValue = BinaryPrimitives.ReverseEndianness(rawValue);
					}
					Buffer!.Replace(StartPosition, rawValue);
					return true;
				}
				return false;
			}
		}

		public class DoubleInterpretation : Interpretation {
			protected override int RequiredLength => sizeof(double);
			public override string Name => "Double";

			public DoubleInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
			}

			protected override string ReadValue() =>
				(ByteOrder switch
				{
					ByteOrder.LittleEndian => Buffer!.ReadDouble(StartPosition),
					ByteOrder.BigEndian => Buffer!.ReadDoubleBigEndian(StartPosition),
					_ => throw new ArgumentOutOfRangeException()
				}).ToString(CultureInfo.CurrentCulture);

			protected override bool TryWriteValue(string value) {
				if (double.TryParse(value, out var doubleValue)) {
					var rawValue = BitConverter.DoubleToInt64Bits(doubleValue);
					if (NeedByteOrderSwap) {
						rawValue = BinaryPrimitives.ReverseEndianness(rawValue);
					}
					Buffer!.Replace(StartPosition, rawValue);
					return true;
				}
				return false;
			}
		}

		public class BinaryInterpretation : Interpretation {
			protected override int RequiredLength => sizeof(byte);
			public override string Name => "Binary (8-bit)";

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
						rawValue |= (byte) (value[value.Length - 1 - i] == '1' ? 1 << i : 0);
					}
					Buffer!.Replace(StartPosition, rawValue);
					return true;
				}
				return false;
			}
		}

		public class GuidInterpretation : Interpretation {
			protected override int RequiredLength => 16;
			public override string Name => "GUID";

			public GuidInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
			}

			protected override string ReadValue() {
				var bytes = Buffer!.ReadBytes(StartPosition, RequiredLength);
				if (NeedByteOrderSwap) {
					Array.Reverse(bytes);
				}
				return new Guid(bytes).ToString();
			}

			protected override bool TryWriteValue(string value) {
				if (Guid.TryParse(value, out var guidValue)) {
					var bytes = guidValue.ToByteArray();
					if (NeedByteOrderSwap) {
						Array.Reverse(bytes);
					}
					Buffer!.Replace(StartPosition, bytes);
					return true;
				}
				return false;
			}
		}

		public class VarIntInterpretation : Interpretation {
			protected override int RequiredLength => 1;
			public override string Name => "VarInt";
			public override bool CanWrite => false;

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

		public class StringInterpretation : Interpretation {
			const int MAX_CHARS = 50;
			static readonly char[] SPLIT_CHARS = {'\0', '\r', '\n', '�'};

			protected override int RequiredLength => 1;
			public override string Name => "String";
			public override bool CanWrite => false;

			public StringInterpretation(HexInspectorViewModel parentViewModel) : base(parentViewModel) {
			}

			protected override string ReadValue() {
				var length = Math.Min((ulong)ParentViewModel.encoding.GetMaxByteCount(MAX_CHARS), ParentViewModel.HexBufferSpan.Length.ToUInt64());
				var bytes = Buffer!.ReadBytes(StartPosition, length);
				var str = ParentViewModel.encoding.GetString(bytes);
				var splitIndex = str.IndexOfAny(SPLIT_CHARS);
				return splitIndex >= 0 ? str.Remove(splitIndex) : str;
			}

			protected override bool TryWriteValue(string value) => false;
		}
	}
}
