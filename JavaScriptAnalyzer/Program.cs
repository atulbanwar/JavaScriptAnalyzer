using JavaScriptAnalyzer.Analyzer;
using JavaScriptAnalyzer.POCO;
using System;
using System.Collections.Generic;

namespace JavaScriptAnalyzer
{
	class Program
	{
		static void Main(string[] args)
		{
			string fileName;

			Console.Write("Enter JavaScript file name (or full file path) with extension: ");
			fileName = Console.ReadLine();

			if (Helper.isValidFile(fileName))
			{
				Dictionary<int, string> extraOrMissingCurlyBrackets = CurlyBracketsAnalyzer.GetExtraOrMissingCurlyBrackets(fileName);

				if (extraOrMissingCurlyBrackets.Count > 0)
				{
					DisplayExtraOrMissingCurlyBrackets(extraOrMissingCurlyBrackets);
				}
				// Check for other errors if no extra or missing brackets count.
				// As missing/extra curly brackets will change the scope of variables/functions/classes
				else
				{
					CodeBlock root = CodeBlockGraphBuilder.GetCodeBlockGraph(fileName);

					VariableUsageAnalyzer.DisplayUnUsedVariables(root, fileName);

					FunctionUsageAnalyzer.DisplayUnDeclaredFunctions(root, fileName);

					SingleLineIfElseAnalyzer.DisplaySingleLineIfElse(fileName);
				}
			}

			Console.ReadLine();
		}

		private static void DisplayExtraOrMissingCurlyBrackets(Dictionary<int, string> extraOrMissingCurlyBrackets)
		{
			if (extraOrMissingCurlyBrackets.Count > 0)
			{
				Console.WriteLine("\nList of missing/extra curly bracket status: ");
				foreach (var extraOrMissingCurlyBracket in extraOrMissingCurlyBrackets)
				{
					Console.WriteLine("Status: " + extraOrMissingCurlyBracket.Value + "\t\t Line No.: " + extraOrMissingCurlyBracket.Key);
				}
			}
		}
	}
}