using System.Collections.Generic;
using System.IO;

namespace JavaScriptAnalyzer.Analyzer
{
	class CurlyBracketsAnalyzer
	{
		public static List<string> GetExtraOrMissingCurlyBrackets(string fileName)
		{
			List<string> extraOrMissingCurlyBrackets = new List<string>();
			string line;
			int lineNo = 0;

			using (StreamReader file = new StreamReader(fileName))
			{
				int activeOpenCurlyBrackets = 0;
				int activeOpenCurlyBracketsBeforeClass = 0;
				bool isParsingClass = false;
				bool isLookingForOpenBracket = false;
				int lineNoExpectingOpenBracket = 0;
				bool isLookingForCorrespondingClosingBracket = false;
				int lineNoExpectingCorrespondingClosingBracket = 0;

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
						extraOrMissingCurlyBrackets.Add("Status: Extra '}' Bracket\t\t\t\t Line No.: " + lineNo);
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
							extraOrMissingCurlyBrackets.Add("Status: Missing '{' Bracket\t\t\t\t Line No.: " + lineNoExpectingOpenBracket);
							// Incrementing 'activeOpenCurlyBrackets' to parse further code as if everything was okay so far.
							activeOpenCurlyBrackets++;
						}
					}

					// Different section as syntax inside class is little different
					if (isParsingClass && (activeOpenCurlyBrackets == activeOpenCurlyBracketsBeforeClass))
					{
						isParsingClass = false;
						isLookingForCorrespondingClosingBracket = false;
						lineNoExpectingCorrespondingClosingBracket = 0;
					}

					if (isParsingClass)
					{
						// While parsing class, if encounter a class or function declaration, it implies class in not closed
						if (LineParserUtil.HasClassDeclaration(line) || LineParserUtil.HasFunctionDeclaration(line))
						{
							extraOrMissingCurlyBrackets.Add("Status: Missing corresponding '}' Bracket\t\t Line No.: " + lineNoExpectingCorrespondingClosingBracket);
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
									extraOrMissingCurlyBrackets.Add("Status: Missing corresponding '}' Bracket\t\t Line No.: " + lineNoExpectingCorrespondingClosingBracket);
									// Decrementing 'activeOpenCurlyBrackets' to parse further code as if everything was okay so far.
									activeOpenCurlyBrackets--;
								}
							}
							else
							{
								// +3 class open bracket, previous block open bracket, current block open bracket
								if (activeOpenCurlyBrackets == activeOpenCurlyBracketsBeforeClass + 3)
								{
									extraOrMissingCurlyBrackets.Add("Status: Missing corresponding '}' Bracket\t\t Line No.: " + lineNoExpectingCorrespondingClosingBracket);
									// Decrementing 'activeOpenCurlyBrackets' to parse further code as if everything was okay so far.
									activeOpenCurlyBrackets--;
								}
							}

							isLookingForCorrespondingClosingBracket = true;
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

				if (activeOpenCurlyBrackets > 0)
				{
					extraOrMissingCurlyBrackets.Add("Status: Missing '}' Bracket\t\t\t\t Line No.: " + lineNo);
				}
			}

			return extraOrMissingCurlyBrackets;
		}
	}
}