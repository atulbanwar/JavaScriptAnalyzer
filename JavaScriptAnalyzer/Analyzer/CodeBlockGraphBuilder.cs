using JavaScriptAnalyzer.Analyzer;
using JavaScriptAnalyzer.POCO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using static JavaScriptAnalyzer.Util.Globals;

namespace JavaScriptAnalyzer
{
	class CodeBlockGraphBuilder
	{
		/// <summary>
		/// Prepares a graph for CodeBlock (A Code block can be a function block, class block, or an open code block.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns>CodeBlock</returns>
		public static CodeBlock GetCodeBlockGraph(string fileName)
		{
			CodeBlock root = GetRootCodeBlock();
			CodeBlock currentCodeBlock = root;
			string line;
			int lineNo = 1;

			using (StreamReader file = new StreamReader(fileName))
			{
				while ((line = file.ReadLine()) != null)
				{
					// Function declaration in Class is different that function declaration at other places.
					// So handling function declaration case inside class seperately
					if (currentCodeBlock.Type == CodeBlockType.Class)
					{
						// If line inside class has a function declaration
						if (CodeBlockGraphUtil.HasClassFunctionDeclaration(line))
						{
							// Making new code block as current code block
							CodeBlock block = GetClassFunctionCodeBlock(line, lineNo);
							UpdateCurrentCodeBlock(ref block, ref currentCodeBlock);
						}
					}
					else
					{
						// If line has a function declaration
						if (CodeBlockGraphUtil.HasFunctionDeclaration(line))
						{
							// Making new code block as current code block
							CodeBlock block = GetFunctionCodeBlock(line, lineNo);
							UpdateCurrentCodeBlock(ref block, ref currentCodeBlock);
						}
						// If the line has a class declaration
						else if (CodeBlockGraphUtil.HasClassDeclaration(line))
						{
							// Making new code block as current code block
							CodeBlock block = GetClassCodeBlock(line, lineNo);
							UpdateCurrentCodeBlock(ref block, ref currentCodeBlock);
						}
						// If the line has variable declaration(s)
						else if (CodeBlockGraphUtil.HasVariableDeclaration(line))
						{
							currentCodeBlock.Variables.AddRange(GetVariables(line, lineNo));
						}
					}

					// Maintaining lines nos where the current block spans.
					currentCodeBlock.RunsOnLines.Add(lineNo);

					if (currentCodeBlock.Type == CodeBlockType.Function || currentCodeBlock.Type == CodeBlockType.Class)
					{
						// Maintaining open parenthesis count to determine when the current code block ends
						if (line.Contains("{"))
						{
							currentCodeBlock.OpenParenthesisCount++;
						}

						if (line.Contains("}"))
						{
							currentCodeBlock.OpenParenthesisCount--;

							if (currentCodeBlock.OpenParenthesisCount.Equals(0))
							{
								// current code block ends, making parent code block as current code block
								currentCodeBlock = currentCodeBlock.ParentBlock;
							}
						}
					}

					lineNo++;
				}
			}

			return root;
		}

		/// <summary>
		/// Get CodeBlock object for root element
		/// </summary>
		/// <returns>CodeBlock</returns>
		private static CodeBlock GetRootCodeBlock()
		{
			return new CodeBlock()
			{
				Name = String.Empty,
				Type = CodeBlockType.Open,
				SubType = CodeBlockSubType.Simple,
				IsRoot = true,
				RunsOnLines = new List<int>(),
				ParentBlock = null,
				ChildrenBlocks = new List<CodeBlock>(),
				Variables = new List<Variable>()
			};
		}

		/// <summary>
		/// Get CodeBlock object for function code block
		/// </summary>
		/// <returns>CodeBlock</returns>
		private static CodeBlock GetFunctionCodeBlock(string line, int lineNo)
		{
			string functionName = string.Empty;
			CodeBlockSubType subType = CodeBlockSubType.Simple;
			Match match = null;

			if (line.ToLower().Replace(" ", "").Contains("=function("))
			{
				subType = CodeBlockSubType.VariableDefined;

				if (line.TrimStart().IndexOf("var ") == 0)
				{
					match = Regex.Match(line, @"var ([a-zA-Z_$][0-9a-zA-Z_]*)", RegexOptions.IgnoreCase);
				}
				else
				{
					match = Regex.Match(line, @"let ([a-zA-Z_$][0-9a-zA-Z_]*)", RegexOptions.IgnoreCase);
				}
			}
			else if (line.TrimStart().IndexOf("function") == 0)
			{
				match = Regex.Match(line, @"function ([a-zA-Z_$][0-9a-zA-Z_$]*)", RegexOptions.IgnoreCase);
			}

			if (match != null && match.Success)
			{
				functionName = match.Groups[1].Value;
			}
			else
			{
				// TODO: Log "Unable to read function name at line ...";
			}

			return new CodeBlock()
			{
				Name = functionName,
				Type = CodeBlockType.Function,
				SubType = subType,
				IsRoot = false,
				RunsOnLines = new List<int>(),
				ParentBlock = null,
				ChildrenBlocks = new List<CodeBlock>(),
				Variables = new List<Variable>()
			};
		}

		/// <summary>
		/// Get CodeBlock object for function code block
		/// </summary>
		/// <returns>CodeBlock</returns>
		private static CodeBlock GetClassFunctionCodeBlock(string line, int lineNo)
		{
			string functionName = string.Empty;
			Match match = null;

			match = Regex.Match(line.TrimStart(), @"([a-zA-Z_$][0-9a-zA-Z_]*)", RegexOptions.IgnoreCase);
			
			if (match.Success)
			{
				functionName = match.Groups[1].Value;
			}
			else
			{
				// TODO: Log "Unable to read function name at line ...";
			}

			return new CodeBlock()
			{
				Name = functionName,
				Type = CodeBlockType.Function,
				SubType = CodeBlockSubType.None,
				IsRoot = false,
				RunsOnLines = new List<int>(),
				ParentBlock = null,
				ChildrenBlocks = new List<CodeBlock>(),
				Variables = new List<Variable>()
			};
		}

		/// <summary>
		/// Get CodeBlock object for class code block
		/// </summary>
		/// <returns>CodeBlock</returns>
		private static CodeBlock GetClassCodeBlock(string line, int lineNo)
		{
			string className = string.Empty;
			Match match = null;

			match = Regex.Match(line, @"class ([a-zA-Z_$][0-9a-zA-Z_]*)", RegexOptions.IgnoreCase);

			if (match != null && match.Success)
			{
				className = match.Groups[1].Value;
			}
			else
			{
				// TODO: Log "Unable to read class name at line ...";
			}

			return new CodeBlock()
			{
				Name = className,
				Type = CodeBlockType.Class,
				SubType = CodeBlockSubType.None,
				IsRoot = false,
				RunsOnLines = new List<int>(),
				ParentBlock = null,
				ChildrenBlocks = new List<CodeBlock>(),
				Variables = new List<Variable>()
			};
		}

		/// <summary>
		/// Fetch variable names from line and return variable objects
		/// </summary>
		/// <param name="line"></param>
		/// <param name="lineNo"></param>
		private static List<Variable> GetVariables(string line, int lineNo)
		{
			List<Variable> variables = new List<Variable>();
			Variable variable = null;
			Match match;
			string variableName;
			line = line.Trim();
			string objectName = string.Empty;
			VariableType type = VariableType.Simple;

			// Checking for multiple variable declaration
			if (line.Contains(","))
			{
				foreach (string varStr in line.Split(','))
				{
					if (CodeBlockGraphUtil.HasVariableDeclaration(varStr))
					{
						if (line.IndexOf("var ") == 0)
						{
							match = Regex.Match(line, @"var ([a-zA-Z_$][0-9a-zA-Z_$]*)", RegexOptions.IgnoreCase);
						}
						else
						{
							match = Regex.Match(line, @"let ([a-zA-Z_$][0-9a-zA-Z_$]*)", RegexOptions.IgnoreCase);
						}
					}
					else
					{
						match = Regex.Match(varStr, @"([a-zA-Z_$][0-9a-zA-Z_$]*)", RegexOptions.IgnoreCase);
					}

					if (match.Success)
					{
						variableName = match.Groups[1].Value;
						variable = new Variable()
						{
							Name = variableName,
							LineNo = lineNo,
							IsUsed = false,
							Type = type,
							ObjectName = objectName
						};

						variables.Add(variable);
					}
					else
					{
						// TODO: Log "Unable to read variable name at line ...";
					}
				}
			}
			else
			{
				if (line.IndexOf("var ") == 0)
				{
					match = Regex.Match(line, @"var ([a-zA-Z_$][0-9a-zA-Z_$]*)", RegexOptions.IgnoreCase);
				}
				else
				{
					match = Regex.Match(line, @"let ([a-zA-Z_$][0-9a-zA-Z_$]*)", RegexOptions.IgnoreCase);
				}

				if (match.Success)
				{
					variableName = match.Groups[1].Value;

					// Looking for object creation
					if (line.Contains("="))
					{
						if (line.Split('=')[1].Trim().IndexOf("new ") == 0)
						{
							match = Regex.Match(line, @"new ([a-zA-Z_$][0-9a-zA-Z_$]*)", RegexOptions.IgnoreCase);

							if (match.Success)
							{
								objectName = match.Groups[1].Value;
								type = VariableType.Object;
							}
						}
					}

					variable = new Variable()
					{
						Name = variableName,
						LineNo = lineNo,
						IsUsed = false,
						Type = type,
						ObjectName = objectName
					};

					variables.Add(variable);
				}
				else
				{
					// TODO: Log "Unable to read variable name";
				}
			}

			return variables;
		}

		/// <summary>
		/// Setting parent code block of new code block as current code block 
		/// Setting children code blocks of current code block as new code block
		/// Making new code block as current code block
		/// </summary>
		/// <param name="block"></param>
		/// <param name="currentCodeBlock"></param>
		private static void UpdateCurrentCodeBlock(ref CodeBlock block, ref CodeBlock currentCodeBlock)
		{
			block.ParentBlock = currentCodeBlock;
			currentCodeBlock.ChildrenBlocks.Add(block);
			currentCodeBlock = block;
		}
	}
}
