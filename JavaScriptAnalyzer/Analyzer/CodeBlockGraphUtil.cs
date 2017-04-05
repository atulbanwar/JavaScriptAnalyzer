using JavaScriptAnalyzer.POCO;

namespace JavaScriptAnalyzer.Analyzer
{
	class CodeBlockGraphUtil
	{
		/// <summary>
		/// Return CurrentCodeBlock based on the line no
		/// </summary>
		/// <param name="currentCodeBlock"></param>
		/// <param name="lineNo"></param>
		/// <returns>CodeBlock</returns>
		public static CodeBlock GetCurrentCodeBlock(CodeBlock currentCodeBlock, int lineNo)
		{
			if (currentCodeBlock.ChildrenBlocks.Count > 0)
			{
				foreach (CodeBlock childCodeBlock in currentCodeBlock.ChildrenBlocks)
				{
					if (childCodeBlock.RunsOnLines.Contains(lineNo))
					{
						return childCodeBlock;
					}
				}
			}

			return currentCodeBlock.ParentBlock;
		}
	}
}
