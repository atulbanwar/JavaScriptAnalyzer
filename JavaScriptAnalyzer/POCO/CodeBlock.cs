using System.Collections.Generic;
using static JavaScriptAnalyzer.Util.Globals;

namespace JavaScriptAnalyzer.POCO
{
	/// <summary>
	/// Represents a block of code
	/// It could be an open code block, a function code block or a class code block
	/// </summary>
	class CodeBlock
	{
		public string Name { get; set; }
		public CodeBlockType Type { get; set; }
		public CodeBlockSubType SubType { get; set; }
		public List<int> RunsOnLines { get; set; }
		public List<Variable> Variables { get; set; }
		public List<CodeBlock> ChildrenBlocks { get; set; }
		public CodeBlock ParentBlock { get; set; }
		public int OpenParenthesisCount { get; set; }
	}
}
