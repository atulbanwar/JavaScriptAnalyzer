using JavaScriptAnalyzer.Analyzer;
using JavaScriptAnalyzer.POCO;
using System;
using System.Collections.Generic;

namespace JavaScriptAnalyzer
{
	class Program
	{
		/// <summary>
		/// JavaScript Analyzer
		/// The program will read the JavaScript file from Input folder. It will report
		/// 1. Extra/Missing Curly Brackets
		/// 2. Varaiables declared but not used
		/// 3. Functions called but not declared
		/// 4. Single line if/else statements
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			string fileName, fileFullPath;

			do
			{
				Console.Clear();
				Console.WriteLine("_____________________ JAVASCRIPT ANALYZER _____________________");
				Console.Write("\nEnter JavaScript file name with extension: ");
				fileName = Console.ReadLine();
				fileFullPath = @"..\..\Input\" + fileName;

				if (Helper.isValidFile(fileFullPath))
				{
					List<string> extraOrMissingCurlyBrackets = CurlyBracketsAnalyzer.GetExtraOrMissingCurlyBrackets(fileFullPath);

					if (extraOrMissingCurlyBrackets.Count > 0)
					{
						DisplayExtraOrMissingCurlyBrackets(extraOrMissingCurlyBrackets);
					}
					// Check for other errors only if no extra or missing brackets found.
					// As missing/extra curly brackets will change the scope of variables/functions/classes
					else
					{
						CodeBlock root = CodeBlockGraphBuilder.GetCodeBlockGraph(fileFullPath);

						VariableUsageAnalyzer.DisplayUnUsedVariables(root, fileFullPath);

						FunctionUsageAnalyzer.DisplayUnDeclaredFunctions(root, fileFullPath);

						SingleLineIfElseAnalyzer.DisplaySingleLineIfElse(fileFullPath);
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