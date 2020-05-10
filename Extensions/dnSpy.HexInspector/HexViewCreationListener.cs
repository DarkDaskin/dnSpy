using System;
using System.ComponentModel.Composition;
using System.Windows;
using dnSpy.Contracts.Hex;
using dnSpy.Contracts.Hex.Editor;
using Microsoft.VisualStudio.Text.Editor;

namespace dnSpy.HexInspector {
	[Export(typeof(WpfHexViewCreationListener))]
	[TextViewRole(PredefinedHexViewRoles.Interactive)]
	public class HexViewCreationListener : WpfHexViewCreationListener {
		readonly ToolWindowContentProvider contentProvider;

		[ImportingConstructor]
		public HexViewCreationListener(ToolWindowContentProvider contentProvider) => this.contentProvider = contentProvider;

		public override void HexViewCreated(WpfHexView hexView) {
			hexView.Closed += OnClosed;
			hexView.VisualElement.IsVisibleChanged += OnIsVisibleChanged;

			void OnClosed(object? sender, EventArgs e) {
				hexView.Closed -= OnClosed;
				hexView.VisualElement.IsVisibleChanged -= OnIsVisibleChanged;
			}

			void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) => OnHexViewVisibilityChanged(hexView);
		}

		void OnHexViewVisibilityChanged(WpfHexView hexView) {
			if (hexView.VisualElement.IsVisible) {
				hexView.Buffer.Changed += OnBufferChanged;
				hexView.Selection.SelectionChanged += OnSelectionChanged;
				hexView.Caret.PositionChanged += OnCaretPositionChanged;

				UpdateInspector(hexView.Buffer, hexView.Caret.Position.Position.ValuePosition.BufferPosition.Position);
			}
			else {
				hexView.Buffer.Changed -= OnBufferChanged;
				hexView.Selection.SelectionChanged -= OnSelectionChanged;
				hexView.Caret.PositionChanged -= OnCaretPositionChanged;

				UpdateInspector(new HexBufferSpan());
			}
		}

		void OnBufferChanged(object? sender, HexContentChangedEventArgs e) => UpdateInspector();

		void OnSelectionChanged(object? sender, EventArgs e) {
			var selection = (HexSelection)sender!;
			if (selection.IsEmpty) {
				UpdateInspector(selection.HexView.Buffer, selection.HexView.Caret.Position.Position.ValuePosition.BufferPosition);
			}
			else {
				UpdateInspector(selection.StreamSelectionSpan);
			}
		}

		void OnCaretPositionChanged(object? sender, HexCaretPositionChangedEventArgs e) {
			if (!e.HexView.Selection.IsEmpty) return;
			UpdateInspector(e.HexView.Buffer, e.NewPosition.Position.ValuePosition.BufferPosition);
		}

		void UpdateInspector() => contentProvider.Content.ViewModel.OnBufferChanged();

		void UpdateInspector(HexBuffer buffer, HexPosition position) =>
			UpdateInspector(HexBufferSpan.FromBounds(
				new HexBufferPoint(buffer, position), new HexBufferPoint(buffer, buffer.Span.End)));

		void UpdateInspector(HexBufferSpan bufferSpan) =>
			contentProvider.Content.ViewModel.HexBufferSpan = bufferSpan;
	}
}
