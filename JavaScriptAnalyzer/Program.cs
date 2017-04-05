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

			do
			{
				Console.Clear();
				Console.WriteLine("_____________________ JAVASCRIPT ANALYZER _____________________");
				Console.Write("\nEnter JavaScript file name (or full file path) with extension: ");
				fileName = Console.ReadLine();

				if (Helper.isValidFile(fileName))
				{
					List<string> extraOrMissingCurlyBrackets = CurlyBracketsAnalyzer.GetExtraOrMissingCurlyBrackets(fileName);

					if (extraOrMissingCurlyBrackets.Count > 0)
					{
						DisplayExtraOrMissingCurlyBrackets(extraOrMissingCurlyBrackets);
					}
					// Check for other errors only if no extra or missing brackets found.
					// As missing/extra curly brackets will change the scope of variables/functions/classes
					else
					{
						CodeBlock root = CodeBlockGraphBuilder.GetCodeBlockGraph(fileName);

						VariableUsageAnalyzer.DisplayUnUsedVariables(root, fileName);

						FunctionUsageAnalyzer.DisplayUnDeclaredFunctions(root, fileName);

						SingleLineIfElseAnalyzer.DisplaySingleLineIfElse(fileName);
					}
				}

				Console.WriteLine("\nPress space bar to run again. Press anything else to exit.");
			} while (Console.ReadKey().Key == ConsoleKey.Spacebar);
		}

		/// <summary>
		/// Displays the list of extra and missing curly brackets
		/// </summary>
		/// <param name="extraOrMissingCurlyBrackets"></param>
		private static void DisplayExtraOrMissingCurlyBrackets(List<string> extraOrMissingCurlyBrackets)
		{
			if (extraOrMissingCurlyBrackets.Count > 0)
			{
				Console.WriteLine("\nList of missing/extra curly brackets: ");
				foreach (string extraOrMissingCurlyBracket in extraOrMissingCurlyBrackets)
				{
					Console.WriteLine(extraOrMissingCurlyBracket);
				}
			}
		}
	}
}