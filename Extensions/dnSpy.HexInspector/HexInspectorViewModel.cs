using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using dnSpy.Contracts.Hex;
using dnSpy.Contracts.MVVM;
using dnSpy.HexInspector.Interpretations;
using dnSpy.HexInspector.Settings;
using Microsoft.VisualStudio.Language.Intellisense;

namespace dnSpy.HexInspector {
	[Export]
	public class HexInspectorViewModel : ViewModelBase, IPartImportsSatisfiedNotification {
		readonly IHexInspectorSettings settings;
		HexBufferSpan hexBufferSpan;
		ByteOrder byteOrder;

		public BulkObservableCollection<Interpretation> Interpretations { get; } = new BulkObservableCollection<Interpretation>();
		public EncodingSelectorViewModel EncodingSelector { get; }

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

		public bool IsByteOrderSelectorVisible => Interpretations.Any(i => i.DependsOnByteOrder);
		public bool IsEncodingSelectorVisible => Interpretations.Any(i => i.DependsOnEncoding);

		internal Encoding Encoding => EncodingSelector.Encoding;

		[ImportingConstructor]
		public HexInspectorViewModel(IHexInspectorSettings settings) {
			this.settings = settings;

			byteOrder = settings.DefaultByteOrder;

			EncodingSelector = new EncodingSelectorViewModel(settings.DefaultCodePage);
			EncodingSelector.PropertyChanged += OnEncodingSelectorPropertyChanged;

			settings.PropertyChanged += OnSettingsPropertyChanged;
		}

		void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == nameof(IHexInspectorSettings.EnabledInterpretations)) {
				Interpretations.Clear();
				InitInterpretations();
			}
		}

		void InitInterpretations() {
			Interpretations.AddRange(settings.EnabledInterpretations.Select(lazy => lazy.Value));
			OnPropertyChanged(nameof(IsByteOrderSelectorVisible));
			OnPropertyChanged(nameof(IsEncodingSelectorVisible));
		}

		void OnEncodingSelectorPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == nameof(EncodingSelectorViewModel.Encoding))
				OnPropertyChanged(nameof(Encoding));
		}

		public void OnBufferChanged() => OnPropertyChanged(nameof(HexBufferSpan));

		// Delay initialization to break circular dependencies.
		public void OnImportsSatisfied() => InitInterpretations();
	}
}
