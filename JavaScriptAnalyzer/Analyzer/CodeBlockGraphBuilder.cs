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
			int lineNo = 0;

			using (StreamReader file = new StreamReader(fileName))
			{
				while ((line = file.ReadLine()) != null)
				{
					Console.WriteLine(line);

					// If line has function declaration
					if (HasFunctionDeclaration(line))
					{
						// Making new code block as current code block
						CodeBlock block = GetFunctionCodeBlock(line, lineNo);
						block.ParentBlock = currentCodeBlock;
						currentCodeBlock.ChildrenBlocks.Add(block);
						currentCodeBlock = block;
					}
					// If the line has variable declaration(s)
					else if (HasVariableDeclaration(line))
					{
						currentCodeBlock.Variables.AddRange(GetVariables(line, lineNo));
					}

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
				LineNo = 0,
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
			String functionName = String.Empty;
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
				LineNo = lineNo,
				ParentBlock = null,
				ChildrenBlocks = new List<CodeBlock>(),
				Variables = new List<Variable>()
			};
		}

		/// <summary>
		/// Checking if the line contains variable declaration(s)
		/// </summary>
		/// <param name="line"></param>
		/// <returns>True if line contains variable declaration else false</returns>
		private static bool HasVariableDeclaration(string line)
		{
			return line.TrimStart().IndexOf("var ") == 0 || line.TrimStart().IndexOf("let ") == 0;
		}

		/// <summary>
		/// Checking if the line contains function declaration
		/// </summary>
		/// <param name="line"></param>
		/// <returns>True if line contains function declaration else false</returns>
		private static bool HasFunctionDeclaration(string line)
		{
			return line.Replace(" ", "").Contains("=function(") || line.TrimStart().IndexOf("function") == 0;
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

			// Checking for multiple variable declaration
			if (line.Contains(","))
			{
				foreach (string varStr in line.Split(','))
				{
					if (HasVariableDeclaration(varStr))
					{
						if (line.TrimStart().IndexOf("var ") == 0)
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
							Type = VariableType.Simple,
							ObjectName = String.Empty
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
				if (line.TrimStart().IndexOf("var ") == 0)
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
					variable = new Variable()
					{
						Name = variableName,
						LineNo = lineNo,
						IsUsed = false,
						Type = VariableType.Simple,
						ObjectName = String.Empty
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
	}
}
