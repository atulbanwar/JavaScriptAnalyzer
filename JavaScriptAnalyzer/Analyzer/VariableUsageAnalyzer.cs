using JavaScriptAnalyzer.POCO;
using System;
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
		/// Display the list of unused variables
		/// </summary>
		/// <param name="root"></param>
		/// <param name="fileName"></param>
		public static void DisplayUnUsedVariables(CodeBlock root, string fileName)
		{
			// First update the usage property of all variables
			UpdateVariableUsageProperty(root, fileName);

			List<Variable> unUsedVariables = new List<Variable>();

			IdentifyUnUsedVariables(unUsedVariables, root);

			unUsedVariables.Sort((x, y) => x.LineNo.CompareTo(y.LineNo));

			if (unUsedVariables.Count > 0)
			{
				Console.WriteLine("\nList of variables declared but not used: ");
				foreach (Variable unUsedVariable in unUsedVariables)
				{
					Console.WriteLine("Name: " + unUsedVariable.Name + "\t\t Line No.: " + unUsedVariable.LineNo);
				}
			}
			else
			{
				Console.WriteLine("\nAll the declared variables are used in the program");
			}
		}

		/// <summary>
		/// Updating IsUsed Property of each variable present in CodeBlocks
		/// </summary>
		/// <param name="root"></param>
		/// <param name="fileName"></param>
		private static void UpdateVariableUsageProperty(CodeBlock root, string fileName)
		{
			CodeBlock currentCodeBlock = root;
			string line;
			int lineNo = 0;

			using (StreamReader file = new StreamReader(fileName))
			{
				while ((line = file.ReadLine()) != null)
				{
					lineNo++;

					if (!currentCodeBlock.RunsOnLines.Contains(lineNo))
					{
						currentCodeBlock = CodeBlockGraphUtil.GetCurrentCodeBlock(currentCodeBlock, lineNo);

						// To skip lines having class, class - function and function declaration
						if (CodeBlockGraphUtil.HasClassDeclaration(line) || CodeBlockGraphUtil.HasFunctionDeclaration(line) || CodeBlockGraphUtil.HasClassFunctionDeclaration(line))
							continue;
					}

					// To skip lines of classblock
					if (currentCodeBlock.Type != CodeBlockType.Class)
					{
						List<string> variablesUsed = GetVariablesUsed(line);

						foreach (string variable in variablesUsed)
						{
							bool isVariableFound = false;
							CodeBlock checkInBlock = currentCodeBlock;

							// Looking for variable from current block in upward direction till root block
							while (isVariableFound != true && checkInBlock != null)
							{
								foreach (Variable declaredVariable in checkInBlock.Variables)
								{
									if (declaredVariable.Name.Equals(variable) && declaredVariable.LineNo < lineNo)
									{
										declaredVariable.IsUsed = true;
										isVariableFound = true;
										break;
									}
								}

								if (isVariableFound == false)
								{
									checkInBlock = checkInBlock.ParentBlock;
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Checking if a variable is used on current line
		/// </summary>
		/// <param name="line"></param>
		/// <returns>true if the current line is making use of some variable else false</returns>
		private static List<string> GetVariablesUsed(string line)
		{
			List<string> variablesUsed = new List<string>();
			line = line.Trim();
			Match match = null;

			// Case I: Empty line, or line contains only '{' or '}' bracket
			if (string.IsNullOrWhiteSpace(line) || line.Equals("}") || line.Equals("{"))
			{
				return variablesUsed;
			}
			// Case II: Variable Declaration with out any value assignment
			else if ((line.IndexOf("var ") == 0 || line.IndexOf("let ") == 0) && !line.Contains("="))
			{
				return variablesUsed;
			}
			// Case II: Variable declaration with value assignment. Can use some variable after '=' sign for assignment
			else if ((line.IndexOf("var ") == 0 || line.IndexOf("let ") == 0) && line.Contains("="))
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
							variablesUsed.AddRange(ExtractVariables(subStr.Split('=')[1].Trim()));
						}
					}
				}
				else
				{
					variablesUsed = ExtractVariables(line.Split('=')[1].Trim());
				}
			}
			// Case IV: Value assignment to a variable
			else if (line.IndexOf("var ") == -1 && line.IndexOf("let ") == -1 && line.Contains("="))
			{
				variablesUsed.Add(line.Split('=')[0].Trim());
				variablesUsed.AddRange(ExtractVariables(line.Split('=')[1].Trim()));
			}
			// Case V: Variable use in return statement
			else if (line.IndexOf("return ") == 0)
			{
				variablesUsed.AddRange(ExtractVariables(line.Remove(0, 7)));
			}
			// Case VI: Function call
			else if (Regex.Match(line, @"([a-zA-Z_$][0-9a-zA-Z_]*)\(", RegexOptions.IgnoreCase).Success)
			{
				// Function call on object variable
				match = Regex.Match(line, @"([a-zA-Z_$][0-9a-zA-Z_]*)\.([a-zA-Z_$][0-9a-zA-Z_]*)\(", RegexOptions.IgnoreCase);
				if (match.Success)
				{
					string objectVariableName = match.Groups[0].Value.Split('.')[0];

					// TODO: Check for other predefined objects
					if (objectVariableName != "Console")
					{
						variablesUsed.Add(objectVariableName);
					}
				}

				variablesUsed.AddRange(ExtractVariables(line));
			}
			// Case VII: Variable++ or Variable-- statements
			else if (Regex.Match(line, @"([a-zA-Z_$][0-9a-zA-Z_.]*)\+\+;|([a-zA-Z_$][0-9a-zA-Z_.]*)\-\-;", RegexOptions.IgnoreCase).Success)
			{
				variablesUsed.AddRange(ExtractVariables(line));
			}

			return variablesUsed;
		}

		/// <summary>
		/// Extract variable names from string
		/// </summary>
		/// <param name="linePart"></param>
		/// <returns>List of variable names</returns>
		private static List<string> ExtractVariables(string linePart)
		{
			List<string> variables = new List<string>();
			Match match = null;
			Regex pattern = null;

			if (string.IsNullOrWhiteSpace(linePart))
				return variables;

			// Creating new object (not using any variable)
			if (linePart.IndexOf("new ") == 0)
				return variables;

			// Function call - Need to consider as some variable may be the part of function's input.
			match = Regex.Match(linePart, @"([a-zA-Z_$][0-9a-zA-Z_.]*)\(", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				string functionName = match.Groups[1].Value;
				linePart = linePart.Replace(functionName, "");
			}

			// Removing (, ), {, }
			pattern = new Regex("[()\\[\\]; ]");
			linePart = pattern.Replace(linePart, "");

			// Replacing all special characters by '|'
			pattern = new Regex("[+\\-*/<>=!,]");
			linePart = pattern.Replace(linePart, "|");

			foreach (var element in linePart.Split('|'))
			{
				int num;
				if (!int.TryParse(element, out num) && !element.Contains("\"") && !element.ToString().Trim().Equals(""))
				{
					variables.Add(element.ToString());
				}
			}

			return variables;
		}

		/// <summary>
		/// Recursively iterate over all code blocks to identify all variables which are not used
		/// </summary>
		/// <param name="unUsedVariables"></param>
		/// <param name="codeBlock"></param>
		private static void IdentifyUnUsedVariables(List<Variable> unUsedVariables, CodeBlock codeBlock)
		{
			foreach (Variable variable in codeBlock.Variables)
			{
				if (!variable.IsUsed)
				{
					unUsedVariables.Add(variable);
				}
			}

			if (codeBlock.ChildrenBlocks.Count > 0)
			{
				foreach (CodeBlock childCodeBlock in codeBlock.ChildrenBlocks)
				{
					IdentifyUnUsedVariables(unUsedVariables, childCodeBlock);
				}
			}
		}
	}
}
