using JavaScriptAnalyzer.POCO;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static JavaScriptAnalyzer.Util.Globals;

namespace JavaScriptAnalyzer.Analyzer
{
	class VariableUsageAnalyzer
	{
		/// <summary>
		/// Updating IsUsed Property of each variable present in CodeBlocks
		/// </summary>
		/// <param name="root"></param>
		/// <param name="fileName"></param>
		public static void UpdateVariableUsageProperty(CodeBlock root, string fileName)
		{
			CodeBlock currentCodeBlock = root;
			string line;
			int lineNo = 1;

			using (StreamReader file = new StreamReader(fileName))
			{
				while ((line = file.ReadLine()) != null)
				{
					if (!currentCodeBlock.RunsOnLines.Contains(lineNo))
					{
						currentCodeBlock = GetCurrentCodeBlock(currentCodeBlock, lineNo);

						if (CodeBlockGraphBuilder.HasClassDeclaration(line) || CodeBlockGraphBuilder.HasFunctionDeclaration(line) || CodeBlockGraphBuilder.HasClassFunctionDeclaration(line))
							continue;
					}

					// To skip lines of classblock and lines having class or function declaration statement
					if (currentCodeBlock.Type != CodeBlockType.Class) {
						List<string> variablesUsed = GetVariablesUsed(line);


					}

					lineNo++;
				}
			}
		}

		/// <summary>
		/// Return CurrentCodeBlock based on the line no
		/// </summary>
		/// <param name="currentCodeBlock"></param>
		/// <param name="lineNo"></param>
		/// <returns>CodeBlock</returns>
		private static CodeBlock GetCurrentCodeBlock(CodeBlock currentCodeBlock, int lineNo)
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
		/// Checking if a variable is used on current line
		/// </summary>
		/// <param name="line"></param>
		/// <returns>true if the current line is making use of some variable else false</returns>
		private static List<string> GetVariablesUsed(string line)
		{
			List<string> variablesUsed = new List<string>();
			Match match = null;
			Regex pattern = null;

			// Case I: Variable Declaration with out any value assignment
			if ((line.TrimStart().IndexOf("var ") == 0 || line.TrimStart().IndexOf("let ") == 0) && !line.Contains("="))
				return variablesUsed;

			// Case II: Variable declaration with value assignment. Can use some variable after '=' sign for assignment
			if ((line.TrimStart().IndexOf("var ") == 0 || line.TrimStart().IndexOf("let ") == 0) && line.Contains("="))
			{
				// Replace ',' present inside '(' and ')' with something to distinguish them with ',' used for multiple variable declaration
				if (line.Contains("(") && line.Contains(","))
				{
					int openBracketCount = 0;
					int index = 0;
					List<int> commaIndexToBeReplaced = new List<int>();

					foreach (char c in line)
					{
						if (c == '(')
							openBracketCount++;

						if (c == ')')
							openBracketCount--;

						if (c == ',' && openBracketCount > 0)
						{
							commaIndexToBeReplaced.Add(index);
						}

						index++;
					}

					StringBuilder sb = new StringBuilder(line);
					foreach (int commaIndex in commaIndexToBeReplaced)
					{
						sb[commaIndex] = '|';
					}
					line = sb.ToString();
				}

				// Multiple Variable Declaration on same line
				if (line.Contains(","))
				{
					foreach (string subStr in line.Split(','))
					{
						// Skip the variable declaration which does not have an assignment part
						if (subStr.Contains("="))
						{
							variablesUsed.AddRange(ExtractVariablesAfterEqualToSign(subStr));
						}
					}
				}
				else
				{
					variablesUsed = ExtractVariablesAfterEqualToSign(line);
				}
			}

			// Case III: Value assignment to a variable
			if (line.TrimStart().IndexOf("var ") == -1 && line.TrimStart().IndexOf("let ") == -1 && line.Contains("="))
			{
				variablesUsed.Add(line.Split('=')[0].Trim());
				variablesUsed.AddRange(ExtractVariablesAfterEqualToSign(line));
			}

			// Case IV: If line has function declaration

			return variablesUsed;
		}

		private static List<string> ExtractVariablesAfterEqualToSign(string linePart)
		{
			List<string> variables = new List<string>();
			Match match = null;
			Regex pattern = null;

			string assignmentPart = linePart.Split('=')[1].ToString().Trim();

			// Case II - I: Assigning new object to variable
			if (assignmentPart.IndexOf("new ") == 0)
				return variables;

			// Case II - II: Assigning some functions' return value.
			// Need to consider as some variable may be the part of function's input.
			match = Regex.Match(assignmentPart, @"([a-zA-Z_$][0-9a-zA-Z_.]*)\(", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				string functionName = match.Groups[1].Value;
				assignmentPart = assignmentPart.Replace(functionName, "");
			}

			// Case II - III: Removing (, ), {, } from file
			pattern = new Regex("[()\\[\\]; ]");
			assignmentPart = pattern.Replace(assignmentPart, "");

			// Case II: IV Replacing all special characters by '|'
			pattern = new Regex("[+\\-*/<>=!,]");
			assignmentPart = pattern.Replace(assignmentPart, "|");

			foreach (var element in assignmentPart.Split('|'))
			{
				int num;
				if (!int.TryParse(element, out num) && !element.Contains("\"") && !element.ToString().Trim().Equals(""))
				{
					variables.Add(element.ToString());
				}
			}

			return variables;
		}
	}
}
