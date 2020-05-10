using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows.Input;
using dnSpy.Contracts.MVVM;
using dnSpy.Contracts.Settings;
using dnSpy.HexInspector.Interpretations;
using Microsoft.VisualStudio.Language.Intellisense;

namespace dnSpy.HexInspector.Settings {
	using InterpretationWithMetadata = Lazy<Interpretation, IInterpretationMetadata>;

	public interface IHexInspectorSettings : INotifyPropertyChanged
	{
		ByteOrder DefaultByteOrder { get; }
		int DefaultCodePage { get; }
		IReadOnlyCollection<InterpretationWithMetadata> EnabledInterpretations { get; }
	}

	public class HexInspectorSettings : ViewModelBase, IHexInspectorSettings {
		ByteOrder defaultByteOrder = ByteOrder.LittleEndian;
		int defaultCodePage = Encoding.UTF8.CodePage;

		public ByteOrder DefaultByteOrder {
			get => defaultByteOrder;
			set {
				if (defaultByteOrder != value) {
					defaultByteOrder = value;
					OnPropertyChanged(nameof(DefaultByteOrder));
				}
			}
		}

		public int DefaultCodePage {
			get => defaultCodePage;
			set {
				if (defaultCodePage != value) {
					defaultCodePage = value;
					OnPropertyChanged(nameof(DefaultCodePage));
				}
			}
		}

		public EncodingSelectorViewModel? EncodingSelector { get; private set; }

		public BulkObservableCollection<InterpretationWithMetadata> AvailableInterpretations { get; } = 
			new BulkObservableCollection<InterpretationWithMetadata>();
		public BulkObservableCollection<InterpretationWithMetadata> EnabledInterpretations { get; } = 
			new BulkObservableCollection<InterpretationWithMetadata>();

		public ICommand EnableAllInterpretationsCommand { get; }
		public ICommand DisableAllInterpretationsCommand { get; }
		public ICommand EnableInterpretationCommand { get; }
		public ICommand DisableInterpretationCommand { get; }
		public ICommand MoveInterpretationUpCommand { get; }
		public ICommand MoveInterpretationDownCommand { get; }
		
		IReadOnlyCollection<InterpretationWithMetadata> IHexInspectorSettings.EnabledInterpretations => EnabledInterpretations;

		protected HexInspectorSettings() {
			EnableAllInterpretationsCommand = new RelayCommand(EnableAllInterpretations, () => AvailableInterpretations.Any());
			DisableAllInterpretationsCommand = new RelayCommand(DisableAllInterpretations, () => EnabledInterpretations.Any());
			EnableInterpretationCommand = new RelayCommand<InterpretationWithMetadata>(EnableInterpretation);
			DisableInterpretationCommand = new RelayCommand<InterpretationWithMetadata>(DisableInterpretation);
			MoveInterpretationUpCommand = new RelayCommand<InterpretationWithMetadata>(
				p => MoveInterpretation(p, -1),
				p => CanMoveInterpretation(p, -1));
			MoveInterpretationDownCommand = new RelayCommand<InterpretationWithMetadata>(
				p => MoveInterpretation(p, 1),
				p => CanMoveInterpretation(p, 1));

			EnabledInterpretations.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(IHexInspectorSettings.EnabledInterpretations));
		}

		protected void Init() {
			if (EncodingSelector != null) {
				EncodingSelector.PropertyChanged -= OnEncodingSelectorPropertyChanged;
			}

			EncodingSelector = new EncodingSelectorViewModel(DefaultCodePage);
			EncodingSelector.PropertyChanged += OnEncodingSelectorPropertyChanged;
		}

		void OnEncodingSelectorPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == nameof(EncodingSelectorViewModel.Encoding)) {
				DefaultCodePage = EncodingSelector!.Encoding.CodePage;
			}
		}
		void EnableAllInterpretations() {
			EnabledInterpretations.AddRange(AvailableInterpretations);
			AvailableInterpretations.Clear();
		}

		void DisableAllInterpretations() {
			AvailableInterpretations.AddRange(EnabledInterpretations);
			EnabledInterpretations.Clear();
		}

		void EnableInterpretation(InterpretationWithMetadata interpretation) {
			EnabledInterpretations.Add(interpretation);
			AvailableInterpretations.Remove(interpretation);
		}

		void DisableInterpretation(InterpretationWithMetadata interpretation) {
			AvailableInterpretations.Add(interpretation);
			EnabledInterpretations.Remove(interpretation);
		}

		bool CanMoveInterpretation(InterpretationWithMetadata interpretation, int offset) {
			var index = EnabledInterpretations.IndexOf(interpretation);
			return index >= 0 && index + offset >= 0 && index + offset < EnabledInterpretations.Count;
		}

		void MoveInterpretation(InterpretationWithMetadata interpretation, int offset) {
			var index = EnabledInterpretations.IndexOf(interpretation);
			EnabledInterpretations.Move(index, index + offset);
		}

		public HexInspectorSettings Clone() => CopyTo(new HexInspectorSettings());

		public HexInspectorSettings CopyTo(HexInspectorSettings other) {
			var supportInitialize = other as ISupportInitialize;
			supportInitialize?.BeginInit();

			other.DefaultByteOrder = DefaultByteOrder;
			other.DefaultCodePage = DefaultCodePage;
			other.AvailableInterpretations.Clear();
			other.AvailableInterpretations.AddRange(AvailableInterpretations);
			other.EnabledInterpretations.Clear();
			other.EnabledInterpretations.AddRange(EnabledInterpretations);
			other.Init();

			supportInitialize?.EndInit();
			return other;
		}
	}

	[Export(typeof(HexInspectorSettings)), Export(typeof(IHexInspectorSettings))]
	public sealed class HexInspectorSettingsImpl : HexInspectorSettings, ISupportInitialize {
		static readonly Guid SETTINGS_GUID = new Guid("4A1F8BAD-BB8B-44D3-B940-2B74EC095FFA");

		readonly ISettingsService settingsService;

		[ImportingConstructor]
		public HexInspectorSettingsImpl(ISettingsService settingsService, [ImportMany] IEnumerable<InterpretationWithMetadata> interpretations) {
			this.settingsService = settingsService;
			
			var section = settingsService.GetOrCreateSection(SETTINGS_GUID);
			DefaultByteOrder = section.Attribute<ByteOrder?>(nameof(DefaultByteOrder)) ?? DefaultByteOrder;
			DefaultCodePage = section.Attribute<int?>(nameof(DefaultCodePage)) ?? DefaultCodePage;
			var enabledInterpretationNames = section.Attribute<string>(nameof(EnabledInterpretations))?.Split(',');

			var allInterpretations =
				(from interpretation in interpretations
				 orderby interpretation.Metadata.DefaultOrder
				 select interpretation).ToList();
			if (enabledInterpretationNames == null) {
				EnabledInterpretations.AddRange(allInterpretations);
			}
			else {
				var enabledInterpretations =
					from interpretation in allInterpretations
					let index = Array.IndexOf(enabledInterpretationNames, interpretation.Metadata.Name)
					where index >= 0
					orderby index
					select interpretation;
				EnabledInterpretations.AddRange(enabledInterpretations);
				AvailableInterpretations.AddRange(allInterpretations.Except(EnabledInterpretations));
			}

			Init();

			PropertyChanged += OnPropertyChanged;
		}

		void OnPropertyChanged(object sender, PropertyChangedEventArgs e) => Save();

		void Save() {
			var section = settingsService.RecreateSection(SETTINGS_GUID);
			section.Attribute(nameof(DefaultByteOrder), DefaultByteOrder);
			section.Attribute(nameof(DefaultCodePage), DefaultCodePage);
			section.Attribute(nameof(EnabledInterpretations), 
				string.Join(",", EnabledInterpretations.Select(i => i.Metadata.Name)));
		}

		public void BeginInit() => PropertyChanged -= OnPropertyChanged;

		public void EndInit() {
			PropertyChanged += OnPropertyChanged;
			Save();
		}
	}
}
