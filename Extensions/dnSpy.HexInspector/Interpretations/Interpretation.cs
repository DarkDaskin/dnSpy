using System;
using System.ComponentModel;
using dnSpy.Contracts.Hex;
using dnSpy.Contracts.MVVM;

namespace dnSpy.HexInspector.Interpretations
{
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
			if (e.PropertyName == nameof(HexInspectorViewModel.HexBufferSpan)) {
				OnPropertyChanged(nameof(IsAvailable));
				OnPropertyChanged(nameof(CanWrite));
			}

			var dependsOnByteOrder = RequiredLength > 1;
			var dependsOnEncoding = this is StringInterpretation;
			if (e.PropertyName == nameof(HexInspectorViewModel.HexBufferSpan) || dependsOnByteOrder && e.PropertyName == nameof(ByteOrder) || dependsOnEncoding && e.PropertyName == nameof(HexInspectorViewModel.Encoding)) {
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
}
