using System;
using System.IO;

namespace JavaScriptAnalyzer
{
	class Util
	{
		public static bool isValidFile(string fileName)
		{
			if (String.IsNullOrWhiteSpace(fileName))
			{
				Console.WriteLine("Please enter valid file name.");
				return false;
			}

			if (!Path.GetExtension(fileName).Equals(".js"))
			{
				Console.WriteLine("The input file is not a valid javascript file.");
				return false;
			}

			if (!File.Exists(fileName))
			{
				Console.WriteLine("File not found. Please enter valid file name with extension (or full file path).");
				return false;
			}

			return true;
		}
	}
}
