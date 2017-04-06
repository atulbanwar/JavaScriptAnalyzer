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
		/// <summary>
		/// Fetches and displays the list of called functions which are not declared
		/// </summary>
		/// <param name="root"></param>
		/// <param name="fileName"></param>
		public static void DisplayUnDeclaredFunctions(CodeBlock root, string fileName)
		{
			List<string> unDeclaredFunctions = GetUnDeclaredFunctionNames(root, fileName);

			Console.WriteLine("\n________ FUNCTION USAGE STATS ________");
			if (unDeclaredFunctions.Count > 0)
			{
				Console.WriteLine("List of functions called but not declared (or out of scope): ");
				foreach (string unDeclaredFn in unDeclaredFunctions)
				{
					Console.WriteLine(unDeclaredFn);
				}
			}
			else
			{
				Console.WriteLine("All functions called are declared.");
			}
		}

		/// <summary>
		/// Finds the list of called functions which are not declared
		/// </summary>
		/// <param name="root"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		private static List<string> GetUnDeclaredFunctionNames(CodeBlock root, string fileName)
		{
			List<string> unDeclaredFunctions = new List<string>();
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

							// Predefined objects
							if (LineParserUtil.IsPredefinedObject(objectVariableName)) continue;

							// Locate variable in current or parent block. If variable is not found in scope, then cannot call function
							Variable objectVariable = GetVariable(objectVariableName, lineNo, currentCodeBlock);

							if (objectVariable == null) continue;

							// If the variable is not of type object, then can't call class methods on top of it
							if (objectVariable.Type != VariableType.Object)
							{
								continue;
							}

							// Predefined methods on an object
							if (LineParserUtil.IsPredefinedObjectFunction(functionName)) continue;

							isFunctionDeclared = IsClassFunctionDeclared(functionName, objectVariable.ObjectName, lineNo, currentCodeBlock);

							// If function is not found in current block and in parents block. Add it to unDeclaredFunctions list
							if (!isFunctionDeclared)
							{
								unDeclaredFunctions.Add("Line No.: " + lineNo + "\t\tName: " + functionName);
							}

							continue;
						}

						// Case III: Function call
						match = Regex.Match(line, @"([a-zA-Z_$][0-9a-zA-Z_]*)\(", RegexOptions.IgnoreCase);
						if (match.Success)
						{
							functionName = match.Groups[1].Value;

							if (LineParserUtil.IsPredefinedFunction(functionName)) continue;

							isFunctionDeclared = IsFunctionDeclared(functionName, lineNo, currentCodeBlock);

							// If function is not found in current block and in parents block. Add it to unDeclaredFunctions list
							if (!isFunctionDeclared)
							{
								unDeclaredFunctions.Add("Line No.: " + lineNo + "\t\tName: " + functionName);
							}
						}
					}
				}
			}

			return unDeclaredFunctions;
		}

		/// <summary>
		/// Checking if the function is defined in class or not
		/// </summary>
		/// <param name="functionName"></param>
		/// <param name="className"></param>
		/// <param name="lineNo"></param>
		/// <param name="currentCodeBlock"></param>
		/// <returns>true if the funtion is defined in class else false</returns>
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

		/// <summary>
		/// Get variable object on which the function is called
		/// </summary>
		/// <param name="objectVariableName"></param>
		/// <param name="lineNo"></param>
		/// <param name="currentCodeBlock"></param>
		/// <returns>Variable Object</returns>
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

		/// <summary>
		/// Checking if the called funtion is declared or not
		/// </summary>
		/// <param name="functionName"></param>
		/// <param name="lineNo"></param>
		/// <param name="currentCodeBlock"></param>
		/// <returns>true if the function is declared a appropriate place or flase</returns>
		private static bool IsFunctionDeclared(string functionName, int lineNo, CodeBlock currentCodeBlock)
		{
			CodeBlock checkInBlock = currentCodeBlock;
			bool isFunctionDeclared = false;

			// Checking for function name in current block childrens
			// While looking at function defined in current block. Check for variable defined functions
			// as they cannot be called before declaration
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
