using System;
using System.ComponentModel.Composition;

namespace dnSpy.HexInspector.Interpretations {
	public interface IInterpretationMetadata {
		string Name { get; }
		string DisplayName { get; }
		bool IsEnabledByDefault { get; }
		int DefaultOrder { get; }
	}

	[MetadataAttribute]
	[AttributeUsage(AttributeTargets.Class)]
	public class ExportInterpretationAttribute : ExportAttribute, IInterpretationMetadata {
		public string Name { get; }
		public string DisplayName { get; set; }
		public bool IsEnabledByDefault { get; set; } = true;
		public int DefaultOrder { get; set; } = 10000;

		public ExportInterpretationAttribute(string name) : base(typeof(Interpretation)) {
			Name = name;
			DisplayName = name;
		}
		
		public ExportInterpretationAttribute(InterpretationType type) : this(type.ToString()) => DefaultOrder = (int)type;
	}
}
