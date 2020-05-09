using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Globalization;
using dnSpy.Contracts.Hex;
using dnSpy.Contracts.MVVM;
using Microsoft.VisualStudio.Language.Intellisense;

namespace dnSpy.HexInspector {
	public class HexInspectorViewModel : ViewModelBase {
		HexBufferSpan hexBufferSpan;
		ByteOrder byteOrder;

		public BulkObservableCollection<Interpretation> Interpretations { get; } = new BulkObservableCollection<Interpretation>();

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

		public HexInspectorViewModel() =>
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
			});

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

						hasError = value != null ? !TryWriteValue(value) : CanWrite;
						HasErrorUpdated();
					}
				}
			}

			public bool IsAvailable => ParentViewModel.HexBufferSpan.Length >= RequiredLength;
			public bool CanWrite => Buffer != null && !Buffer.IsReadOnly && IsAvailable;

			protected HexBuffer? Buffer => ParentViewModel.HexBufferSpan.Buffer;
			protected HexPosition StartPosition => ParentViewModel.HexBufferSpan.Start.Position;
			protected ByteOrder ByteOrder => ParentViewModel.ByteOrder;
			protected bool NeedByteOrderSwap => BitConverter.IsLittleEndian != (ByteOrder == ByteOrder.LittleEndian);

			protected Interpretation(HexInspectorViewModel parentViewModel) {
				ParentViewModel = parentViewModel;
				ParentViewModel.PropertyChanged += OnParentViewModelPropertyChanged;
			}

			protected abstract string ReadValue();
			protected abstract bool TryWriteValue(string value);

			void OnParentViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
				if (e.PropertyName == nameof(HexBufferSpan)) {
					OnPropertyChanged(nameof(IsAvailable));
					OnPropertyChanged(nameof(CanWrite));
				}
				if (e.PropertyName == nameof(HexBufferSpan) || e.PropertyName == nameof(ByteOrder)) {
					value = IsAvailable ? ReadValue() : null;
					OnPropertyChanged(nameof(Value));
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
				if (long.TryParse(value, out var rawValue)) {
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
	}
}
