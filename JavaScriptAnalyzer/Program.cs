using System;
using System.IO;

namespace JavaScriptAnalyzer
{
	class Program
	{
		static void Main(string[] args)
		{
			string fileName;

			Console.Write("Enter JavaScript file name with extension (or full file path): ");
			fileName = Console.ReadLine();

			if (Util.isValidFile(fileName))
			{
				string line;

				StreamReader file = new StreamReader(fileName);
				while ((line = file.ReadLine()) != null)
				{
					Console.WriteLine(line);
				}

				file.Close();
			}

			Console.ReadLine();
		}
	}
}