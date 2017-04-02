using System.Collections.Generic;
using static JavaScriptAnalyzer.Util.Globals;

namespace JavaScriptAnalyzer.POCO
{
	class CodeBlock
	{
		public string Name { get; set; }
		public CodeBlockType Type { get; set; }
		public CodeBlockSubType SubType { get; set; }
		public bool IsRoot { get; set; }
		public int LineNo { get; set; }
		public List<Variable> Variables { get; set; }
		public List<CodeBlock> ChildrenBlocks { get; set; }
		public CodeBlock ParentBlock { get; set; }
	}
}
