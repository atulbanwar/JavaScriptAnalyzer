using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace JavaScriptAnalyzer.Analyzer
{
	class SingleLineIfElseAnalyzer
	{
		/// <summary>
		/// Fetches and displays the list of single line If/Else statements
		/// </summary>
		/// <param name="fileName"></param>
		public static void DisplaySingleLineIfElse(string fileName)
		{
			List<string> singleLineIfElseStatements = GetSingleLineIfElse(fileName);

			Console.WriteLine("\n________ SINGLE LINE IF/ELSE ________");
			if (singleLineIfElseStatements.Count > 0)
			{
				Console.WriteLine("List of single line if / else statements: ");
				foreach (string singleLineIfElse in singleLineIfElseStatements)
				{
					Console.WriteLine(singleLineIfElse);
				}
			}
			else
			{
				Console.WriteLine("No single line if/else statements found.");
			}
		}

		/// <summary>
		/// Fetches the list of single line If/Else statements
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		private static List<string> GetSingleLineIfElse(string fileName)
		{
			List<string> singleLineIfElseStatements = new List<string>();
			string line;
			int lineNo = 0;

			using (StreamReader file = new StreamReader(fileName))
			{
				bool isLookForOpenCurlyBracketForIf = false;
				bool isLookForOpenCurlyBracketForElse = false;
				Match match = null;

				while ((line = file.ReadLine()) != null)
				{
					lineNo++;
					line = line.Trim();

					if (line.Equals(string.Empty)) continue;

					// If '{' of if is not present on last line, then looking on next line
					if (isLookForOpenCurlyBracketForIf)
					{
						isLookForOpenCurlyBracketForIf = !isLookForOpenCurlyBracketForIf;
						if (line[0] != '{')
						{
							singleLineIfElseStatements.Add("Line No.: " + (lineNo - 1) + "\t\tStatement: IF");
						}
					}

					// If '{' of else is not present on last line, then looking on next line
					if (isLookForOpenCurlyBracketForElse)
					{
						isLookForOpenCurlyBracketForElse = !isLookForOpenCurlyBracketForElse;
						if (line[0] != '{')
						{
							singleLineIfElseStatements.Add("Line No.: " + (lineNo - 1) + "\t\tStatement: ELSE");
						}
					}

					// Looking for If statement
					match = Regex.Match(line, @"if\s+\(", RegexOptions.IgnoreCase);
					if (match.Success)
					{
						match = Regex.Match(line, @"\)\s*{", RegexOptions.IgnoreCase);

						if (!match.Success)
						{
							isLookForOpenCurlyBracketForIf = true;
						}
					}

					// Looking for Else statement
					match = Regex.Match(line, @"\belse\b", RegexOptions.IgnoreCase);
					if (match.Success)
					{
						match = Regex.Match(line, @"\belse\s*{", RegexOptions.IgnoreCase);

						if (!match.Success)
						{
							isLookForOpenCurlyBracketForElse = true;
						}
					}
				}
			}

			return singleLineIfElseStatements;
		}
	}
}
