using System.Collections.Generic;
using System.IO;

namespace JavaScriptAnalyzer.Analyzer
{
	class CurlyBracketsAnalyzer
	{
		public static Dictionary<int, string> GetExtraOrMissingCurlyBrackets(string fileName)
		{
			Dictionary<int, string> extraOrMissingCurlyBrackets = new Dictionary<int, string>();
			string line;
			int lineNo = 0;

			using (StreamReader file = new StreamReader(fileName))
			{
				int activeOpenCurlyBrackets = 0;
				int activeOpenCurlyBracketsBeforeClass = 0;
				bool isParsingClass = false;
				bool isLookingForOpenBracket = false;
				int lineNoExpectingOpenBracket = 0;

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
						extraOrMissingCurlyBrackets.Add(lineNo, "Extra '}' Bracket");
						// setting this to 0, to parse further code as if everything was okay so far.
						activeOpenCurlyBrackets = 0;
					}

					// Rule II: A function, Class, Constructor, Class Function, Setter, Getter must have an '{' bracket
					if (isLookingForOpenBracket)
					{
						isLookingForOpenBracket = !isLookingForOpenBracket;
						if (line[0] != '{')
						{
							extraOrMissingCurlyBrackets.Add(lineNoExpectingOpenBracket, "Missing '{' Bracket");
							// Incrementing 'activeOpenCurlyBrackets' to parse further code as if everything was okay so far.
							activeOpenCurlyBrackets++;
						}
					}

					// Different section as syntax inside class is little different
					if (isParsingClass)
					{
						if (activeOpenCurlyBrackets == activeOpenCurlyBracketsBeforeClass)
						{
							isParsingClass = false;
						}

						if (LineParserUtil.HasConstructorDeclaration(line) || LineParserUtil.HasSetterDeclaration(line) || LineParserUtil.HasGetterDeclaration(line) || LineParserUtil.HasClassFunctionDeclaration(line))
						{
							if (!line.Contains("{"))
							{
								isLookingForOpenBracket = true;
								lineNoExpectingOpenBracket = lineNo;
							}
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
					extraOrMissingCurlyBrackets.Add(lineNo, "Missing '}' Bracket");
				}
			}

			return extraOrMissingCurlyBrackets;
		}
	}
}