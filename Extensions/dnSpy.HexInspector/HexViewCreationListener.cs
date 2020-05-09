using System;
using System.ComponentModel.Composition;
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
			hexView.Closed += OnHexViewClosed;
			hexView.Buffer.Changed += OnBufferChanged;
			hexView.Caret.PositionChanged += OnCaretPositionChanged;
		}

		void OnHexViewClosed(object? sender, EventArgs e) {
			var hexView = (WpfHexView)sender!;
			hexView.Closed -= OnHexViewClosed;
			hexView.Buffer.Changed -= OnBufferChanged;
			hexView.Caret.PositionChanged -= OnCaretPositionChanged;
		}

		void OnBufferChanged(object? sender, HexContentChangedEventArgs e) => UpdateInspector();

		void OnCaretPositionChanged(object? sender, HexCaretPositionChangedEventArgs e) => 
			UpdateInspector(e.HexView.Buffer, e.NewPosition.Position.ValuePosition.BufferPosition.Position);

		void UpdateInspector() => contentProvider.Content.ViewModel.OnBufferChanged();

		void UpdateInspector(HexBuffer buffer, HexPosition position) =>
			contentProvider.Content.ViewModel.HexBufferSpan = HexBufferSpan.FromBounds(
				new HexBufferPoint(buffer, position), new HexBufferPoint(buffer, buffer.Span.End));
	}
}
