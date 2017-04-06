using System;
using System.IO;

namespace JavaScriptAnalyzer
{
	class Helper
	{
		/// <summary>
		/// Checking if the input file is a valid file
		/// </summary>
		/// <param name="fileFullPath"></param>
		/// <returns></returns>
		public static bool isValidFile(string fileFullPath)
		{
			if (!Path.GetExtension(fileFullPath).Equals(".js"))
			{
				Console.WriteLine("The input file is not a valid javascript file.");
				return false;
			}

			if (!File.Exists(fileFullPath))
			{
				Console.WriteLine("File not found. Please enter JavaScript file name with extension.");
				return false;
			}

			return true;
		}
	}
}
