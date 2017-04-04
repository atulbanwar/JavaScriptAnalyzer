using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace JavaScriptAnalyzer.Analyzer
{
	class SingleLineIfElseAnalyzer
	{
		public static void DisplaySingleLineIfElse(string fileName)
		{
			Dictionary<int, string> singleLineIfElseStatements = GetSingleLineIfElse(fileName);

			if (singleLineIfElseStatements.Count > 0)
			{
				Console.WriteLine("\nList of single line if / else statements: ");
				foreach (var singleLineIfElse in singleLineIfElseStatements)
				{
					Console.WriteLine("Statement: " + singleLineIfElse.Value + "\t\t Line No.: " + singleLineIfElse.Key);
				}
			}
			else
			{
				Console.WriteLine("\nNo single line if/else statements found.");
			}
		}

		private static Dictionary<int, string> GetSingleLineIfElse(string fileName)
		{
			Dictionary<int, string> singleLineIfElseStatements = new Dictionary<int, string>();
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

					// If '{' of if is not present on last line, then looking on next line
					if (isLookForOpenCurlyBracketForIf)
					{
						isLookForOpenCurlyBracketForIf = !isLookForOpenCurlyBracketForIf;
						if (line[0] != '{')
						{
							singleLineIfElseStatements.Add(lineNo - 1, "IF");
						}
					}

					// If '{' of else is not present on last line, then looking on next line
					if (isLookForOpenCurlyBracketForElse)
					{
						isLookForOpenCurlyBracketForElse = !isLookForOpenCurlyBracketForElse;
						if (line[0] != '{')
						{
							singleLineIfElseStatements.Add(lineNo - 1, "ELSE");
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
