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

			if (unDeclaredFunctions.Count > 0)
			{
				Console.WriteLine("\nList of functions called but not declared (or out of scope): ");
				foreach (var unDeclaredFn in unDeclaredFunctions)
				{
					Console.WriteLine("Name: " + unDeclaredFn.Key + "\t\t Line No.: " + unDeclaredFn.Value);
				}
			}
			else
			{
				Console.WriteLine("\nAll functions called are declared.");
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
						if (LineParserUtil.HasClassDeclaration(line) || LineParserUtil.HasFunctionDeclaration(line) || LineParserUtil.HasClassFunctionDeclaration(line))
							continue;
					}

					// To skip lines of classblock
					if (currentCodeBlock.Type != CodeBlockType.Class)
					{
						string functionName = "";
						string objectVariableName = "";
						bool isFunctionDeclared = false;
						Match match = null;

						// Case I: No funcion call. Object creation let a = new abc();
						if (Regex.Match(line, @"let\s+([a-zA-Z_$][0-9a-zA-Z_]*)\s+=\s+new\s+", RegexOptions.IgnoreCase).Success)
						{
							continue;
						}

						// Case II: Class function call
						match = Regex.Match(line, @"([a-zA-Z_$][0-9a-zA-Z_]*)\.([a-zA-Z_$][0-9a-zA-Z_]*)\(", RegexOptions.IgnoreCase);
						if (match.Success)
						{
							string[] functionCallParts = match.Groups[0].Value.Split('.');
							objectVariableName = functionCallParts[0];
							functionName = functionCallParts[1].Replace("(", "");

							// TODO: Check for other predefined objects
							if (objectVariableName == "console")
							{
								continue;
							}

							// Locate variable in current or parent block. If variable is not found in scope, then cannot call function
							Variable objectVariable = GetVariable(objectVariableName, lineNo, currentCodeBlock);

							// TODO: Handle predefinded methods which can be called on any variables
							// If the variable is not of type object, then can't call class methods on top of it
							if (objectVariable.Type != VariableType.Object)
							{
								continue;
							}

							isFunctionDeclared = IsClassFunctionDeclared(functionName, objectVariable.ObjectName, lineNo, currentCodeBlock);

							// If function is not found in current block and in parents block. Add it to unDeclaredFunctions list
							if (!isFunctionDeclared)
							{
								unDeclaredFunctions.Add(functionName, lineNo);
							}

							continue;
						}

						// Case III: Function call
						match = Regex.Match(line, @"([a-zA-Z_$][0-9a-zA-Z_]*)\(", RegexOptions.IgnoreCase);
						if (match.Success)
						{
							functionName = match.Groups[1].Value;
							isFunctionDeclared = IsFunctionDeclared(functionName, lineNo, currentCodeBlock);

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

		private static bool IsClassFunctionDeclared(string functionName, string className, int lineNo, CodeBlock currentCodeBlock)
		{
			CodeBlock checkInBlock = currentCodeBlock;
			bool isFunctionDeclared = false;
			bool isClassFound = false;

			// First find the class code block
			// Checking for class name in current block childrens
			foreach (CodeBlock childCodeBlock in checkInBlock.ChildrenBlocks)
			{
				if (childCodeBlock.Type == CodeBlockType.Class && childCodeBlock.Name == className)
				{
					checkInBlock = childCodeBlock;
					isClassFound = true;
					break;
				}
			}

			// Check child code block of parent code block in upward direction to locate function declaration
			if (!isClassFound)
			{
				checkInBlock = checkInBlock.ParentBlock;
				while (!isClassFound && checkInBlock != null)
				{
					foreach (CodeBlock childCodeBlock in checkInBlock.ChildrenBlocks)
					{
						if (childCodeBlock.Type == CodeBlockType.Class && childCodeBlock.Name == className)
						{
							checkInBlock = childCodeBlock;
							isClassFound = true;
							break;
						}
					}

					if (!isClassFound)
						checkInBlock = checkInBlock.ParentBlock;
				}
			}

			// If class found, then look for the function name in class childrens code blocks
			if (isClassFound)
			{
				foreach (CodeBlock childCodeBlock in checkInBlock.ChildrenBlocks)
				{
					if (childCodeBlock.Type == CodeBlockType.Function && childCodeBlock.Name == functionName)
					{
						isFunctionDeclared = true;
						break;
					}
				}
			}

			return isFunctionDeclared;
		}

		private static Variable GetVariable(string objectVariableName, int lineNo, CodeBlock currentCodeBlock)
		{
			Variable variable = null;
			CodeBlock checkInBlock = currentCodeBlock;

			foreach (Variable declaredVariable in checkInBlock.Variables)
			{
				if (declaredVariable.Name.Equals(objectVariableName) && declaredVariable.LineNo < lineNo)
				{
					variable = declaredVariable;
					break;
				}
			}

			// Looking for variable from current block in upward direction till root block
			checkInBlock = checkInBlock.ParentBlock;
			while (variable == null && checkInBlock != null)
			{
				foreach (Variable declaredVariable in checkInBlock.Variables)
				{
					if (declaredVariable.Name.Equals(objectVariableName))
					{
						variable = declaredVariable;
						break;
					}
				}

				if (variable == null)
				{
					checkInBlock = checkInBlock.ParentBlock;
				}
			}

			return variable;
		}

		private static bool IsFunctionDeclared(string functionName, int lineNo, CodeBlock currentCodeBlock)
		{
			CodeBlock checkInBlock = currentCodeBlock;
			bool isFunctionDeclared = false;

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

			return isFunctionDeclared;
		}
	}
}
