using JavaScriptAnalyzer.POCO;
using System;

namespace JavaScriptAnalyzer
{
	class Program
	{
		static void Main(string[] args)
		{
			string fileName;

			Console.Write("Enter JavaScript file name (or full file path) with extension: ");
			fileName = Console.ReadLine();

			if (Helper.isValidFile(fileName))
			{
				CodeBlock root = CodeBlockGraphBuilder.GetCodeBlockGraph(fileName);
			}

			Console.ReadLine();
		}
	}
}