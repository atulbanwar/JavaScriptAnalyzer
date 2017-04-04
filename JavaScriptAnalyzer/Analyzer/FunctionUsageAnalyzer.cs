using JavaScriptAnalyzer.POCO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using static JavaScriptAnalyzer.Util.Globals;

namespace JavaScriptAnalyzer.Analyzer
{
	class FunctionUsageAnalyzer
	{
		public static void DisplayUnDeclaredFunctions(CodeBlock root, string fileName)
		{
			
			Dictionary<string, int> unDeclaredFunctions = GetUnDeclaredFunctionNames(root, fileName);

			Console.WriteLine("\nList of functions called but not declared (or out of scope): ");
			foreach(var unDeclaredFn in unDeclaredFunctions)
			{
				Console.WriteLine("Name: " + unDeclaredFn.Key + "\t\t Line No.: " + unDeclaredFn.Value);
			}
			
		}

		private static Dictionary<string, int> GetUnDeclaredFunctionNames(CodeBlock root, string fileName)
		{
			Dictionary<string, int> unDeclaredFunctions = new Dictionary<string, int>();
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
						List<string> calledFunctionNames = GetCalledFunctionNames(line);

						foreach (string functionName in calledFunctionNames)
						{
							bool isFunctionDeclared = false;

							CodeBlock checkInBlock = currentCodeBlock;

							// Checking for function name in current block childrens
							// While looking at function defined in current block. Check for variable defined functions
							// as they cannot be called before they are declared
							foreach (CodeBlock childCodeBlock in checkInBlock.ChildrenBlocks)
							{
								if (childCodeBlock.Type == CodeBlockType.Function && childCodeBlock.Name == functionName)
								{
									if (childCodeBlock.SubType == CodeBlockSubType.VariableDefined)
									{
										if (childCodeBlock.RunsOnLines[0] < lineNo)
										{
											isFunctionDeclared = true;
											break;
										}
									}
									else
									{
										isFunctionDeclared = true;
										break;
									}
								}
							}

							// Check child code block of parent code block in upward direction to locate function declaration
							checkInBlock = checkInBlock.ParentBlock;
							while (!isFunctionDeclared && checkInBlock != null)
							{
								foreach (CodeBlock childCodeBlock in checkInBlock.ChildrenBlocks)
								{
									if (childCodeBlock.Type == CodeBlockType.Function && childCodeBlock.Name == functionName)
									{
										isFunctionDeclared = true;
										break;
									}
								}

								checkInBlock = checkInBlock.ParentBlock;
							}

							// If function is not found in current block and in parents block. Add it to unDeclaredFunctions list
							if (!isFunctionDeclared)
							{
								unDeclaredFunctions.Add(functionName, lineNo);
							}
						}
					}
				}
			}

			return unDeclaredFunctions;
		}

		private static List<string> GetCalledFunctionNames(string line)
		{
			List<string> functionNames = new List<string>();
			Match match = null;

			match = Regex.Match(line, @"([a-zA-Z_$][0-9a-zA-Z_.]*)\(", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				string functionName = match.Groups[1].Value;
				functionNames.Add(functionName);
			}

			return functionNames;
		}
	}
}
