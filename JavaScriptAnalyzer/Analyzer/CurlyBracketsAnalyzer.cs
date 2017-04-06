using System.Collections.Generic;
using System.IO;

namespace JavaScriptAnalyzer.Analyzer
{
	class CurlyBracketsAnalyzer
	{
		/// <summary>
		/// Find and return the list of Extra/Missing curly brackets
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns>List<string></returns>
		public static List<string> GetExtraOrMissingCurlyBrackets(string fileName)
		{
			List<string> extraOrMissingCurlyBrackets = new List<string>();
			string line;
			int lineNo = 0;

			using (StreamReader file = new StreamReader(fileName))
			{
				int activeOpenCurlyBrackets = 0;
				int activeOpenCurlyBracketsBeforeClass = 0;
				int lineNoExpectingOpenBracket = 0;
				int lineNoExpectingCorrespondingClosingBracket = 0;
				bool isParsingClass = false;
				bool isLookingForOpenBracket = false;

				while ((line = file.ReadLine()) != null)
				{
					lineNo++;
					line = line.Trim();

					if (line.Equals(string.Empty)) continue;

					// Counting number of '{' and '}' brackets and maintaining activeOpenCurlyBracketCount
					if (line.Contains("{") && line.Contains("}"))
					{
						foreach (char c in line)
						{
							if (c == '{') activeOpenCurlyBrackets++;
							if (c == '}') activeOpenCurlyBrackets--;
						}
					}
					else if (line.Contains("{"))
					{
						foreach (char c in line)
						{
							if (c == '{') activeOpenCurlyBrackets++;
						}
					}
					else if (line.Contains("}"))
					{
						foreach (char c in line)
						{
							if (c == '}') activeOpenCurlyBrackets--;
						}
					}

					// Rule I: If at anytime 'activeOpenCurlyBrackets' count < 0, it implies that there is an extra '}' bracket
					if (activeOpenCurlyBrackets < 0)
					{
						extraOrMissingCurlyBrackets.Add("Line No.: " + lineNo + "\t\tStatus: Extra '}' Bracket");
						// setting this to 0, to parse further code as if everything was okay so far.
						activeOpenCurlyBrackets = 0;
					}

					// Rule II: A function, Class, Constructor, Class Function, Setter, Getter must have an '{' bracket
					// Rule III: A constructor, Setter, Getter, ClassFunction must have a corresponding closing bracket
					if (isLookingForOpenBracket)
					{
						isLookingForOpenBracket = !isLookingForOpenBracket;
						if (line[0] != '{')
						{
							extraOrMissingCurlyBrackets.Add("Line No.: " + lineNoExpectingOpenBracket + "\t\tStatus: Missing '{' Bracket");
							// Incrementing 'activeOpenCurlyBrackets' to parse further code as if everything was okay so far.
							activeOpenCurlyBrackets++;
						}
					}

					// Different section for class as syntax inside class is different
					if (isParsingClass && (activeOpenCurlyBrackets == activeOpenCurlyBracketsBeforeClass))
					{
						isParsingClass = false;
						lineNoExpectingCorrespondingClosingBracket = 0;
					}

					if (isParsingClass)
					{
						// While parsing class, if encounter a class or function declaration, it implies class in not closed
						if (LineParserUtil.HasClassDeclaration(line) || LineParserUtil.HasFunctionDeclaration(line))
						{
							extraOrMissingCurlyBrackets.Add("Line No.: " + lineNoExpectingCorrespondingClosingBracket + "\t\tStatus: Missing corresponding '}' Bracket");
							// Decrementing 'activeOpenCurlyBrackets' to parse further code as if everything was okay so far.
							activeOpenCurlyBrackets--;
						}

						if (LineParserUtil.HasConstructorDeclaration(line) || LineParserUtil.HasSetterDeclaration(line) ||
							LineParserUtil.HasGetterDeclaration(line) || LineParserUtil.HasClassFunctionDeclaration(line))
						{
							// If a new block starts with this condition, it implies some block is still open
							if (!line.Contains("{"))
							{
								isLookingForOpenBracket = true;
								lineNoExpectingOpenBracket = lineNo;

								// +3 class open bracket, previous block open bracket
								if (activeOpenCurlyBrackets == activeOpenCurlyBracketsBeforeClass + 2)
								{
									extraOrMissingCurlyBrackets.Add("Line No.: " + lineNoExpectingCorrespondingClosingBracket + "\t\tStatus: Missing corresponding '}' Bracket");
									// Decrementing 'activeOpenCurlyBrackets' to parse further code as if everything was okay so far.
									activeOpenCurlyBrackets--;
								}
							}
							else
							{
								// +3 class open bracket, previous block open bracket, current block open bracket
								if (activeOpenCurlyBrackets == activeOpenCurlyBracketsBeforeClass + 3)
								{
									extraOrMissingCurlyBrackets.Add("Line No.: " + lineNoExpectingCorrespondingClosingBracket + "\t\tStatus: Missing corresponding '}' Bracket");
									// Decrementing 'activeOpenCurlyBrackets' to parse further code as if everything was okay so far.
									activeOpenCurlyBrackets--;
								}
							}

							lineNoExpectingCorrespondingClosingBracket = lineNo;
						}
					}
					else
					{
						if (LineParserUtil.HasClassDeclaration(line))
						{
							isParsingClass = true;
							if (line.Contains("{"))
							{
								activeOpenCurlyBracketsBeforeClass = activeOpenCurlyBrackets - 1;
							}
							else
							{
								activeOpenCurlyBracketsBeforeClass = activeOpenCurlyBrackets;
								isLookingForOpenBracket = true;
								lineNoExpectingOpenBracket = lineNo;
							}
						}

						if (LineParserUtil.HasFunctionDeclaration(line))
						{
							if (!line.Contains("{"))
							{
								isLookingForOpenBracket = true;
								lineNoExpectingOpenBracket = lineNo;
							}
						}
					}
				}

				// Rule IV: If in the end the count of open bracket is more, report a missing closing bracket
				if (activeOpenCurlyBrackets > 0)
				{
					extraOrMissingCurlyBrackets.Add("Line No.: " + lineNo + "\t\tStatus: Missing '}' Bracket");
				}
			}

			return extraOrMissingCurlyBrackets;
		}
	}
}