using JavaScriptAnalyzer.POCO;
using System.Text.RegularExpressions;

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

		/// <summary>
		/// Checking if the line contains variable declaration(s)
		/// </summary>
		/// <param name="line"></param>
		/// <returns>True if line contains variable declaration else false</returns>
		public static bool HasVariableDeclaration(string line)
		{
			return line.TrimStart().IndexOf("var ") == 0 || line.TrimStart().IndexOf("let ") == 0;
		}

		/// <summary>
		/// Checking if the line contains function declaration
		/// </summary>
		/// <param name="line"></param>
		/// <returns>True if line contains function declaration else false</returns>
		public static bool HasFunctionDeclaration(string line)
		{
			return line.Replace(" ", "").Contains("=function(") || line.TrimStart().IndexOf("function") == 0;
		}

		/// <summary>
		/// Checking if the line contains a class declaration
		/// </summary>
		/// <param name="line"></param>
		/// <returns>True if line contains class declaration else false</returns>
		public static bool HasClassDeclaration(string line)
		{
			return line.TrimStart().IndexOf("class ") == 0;
		}

		/// <summary>
		/// Checking if the line inside class contains function declaration
		/// </summary>
		/// <param name="line"></param>
		/// <returns>True if line contains function declaration else false</returns>
		public static bool HasClassFunctionDeclaration(string line)
		{
			Match match;
			match = Regex.Match(line, @"([a-zA-Z_$][0-9a-zA-Z_$]*)\(", RegexOptions.IgnoreCase);

			return !(line.TrimStart().IndexOf("set ") == 0 || line.TrimStart().IndexOf("get ") == 0 || line.TrimStart().IndexOf("constructor(") == 0 || line.Contains(";")) && match.Success;
		}
	}
}
