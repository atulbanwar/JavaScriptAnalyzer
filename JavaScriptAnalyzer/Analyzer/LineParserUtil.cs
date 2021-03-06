﻿using System.Text.RegularExpressions;

namespace JavaScriptAnalyzer.Analyzer
{
	class LineParserUtil
	{
		/// <summary>
		/// Checking if the line contains variable declaration(s)
		/// </summary>
		/// <param name="line"></param>
		/// <returns>True if line contains variable declaration else false</returns>
		internal static bool HasVariableDeclaration(string line)
		{
			return line.TrimStart().IndexOf("var ") == 0 || line.TrimStart().IndexOf("let ") == 0;
		}

		/// <summary>
		/// Checking if the line contains function declaration
		/// </summary>
		/// <param name="line"></param>
		/// <returns>True if line contains function declaration else false</returns>
		internal static bool HasFunctionDeclaration(string line)
		{
			return line.Replace(" ", "").Contains("=function(") || line.TrimStart().IndexOf("function") == 0;
		}

		/// <summary>
		/// Checking if the line contains a class declaration
		/// </summary>
		/// <param name="line"></param>
		/// <returns>True if line contains class declaration else false</returns>
		internal static bool HasClassDeclaration(string line)
		{
			return line.TrimStart().IndexOf("class ") == 0;
		}

		/// <summary>
		/// Checking if the line inside class contains function declaration
		/// </summary>
		/// <param name="line"></param>
		/// <returns>True if line contains function declaration else false</returns>
		internal static bool HasClassFunctionDeclaration(string line)
		{
			Match match;
			match = Regex.Match(line, @"([a-zA-Z_$][0-9a-zA-Z_$]*)\(", RegexOptions.IgnoreCase);

			return !(line.TrimStart().IndexOf("set ") == 0 || line.TrimStart().IndexOf("get ") == 0 || line.TrimStart().IndexOf("constructor(") == 0 || line.Contains(";")) && match.Success;
		}

		/// <summary>
		/// Checking if the line contains a class setter declaration
		/// </summary>
		/// <param name="line"></param>
		/// <returns>True if line contains class setter declaration else false</returns>
		internal static bool HasSetterDeclaration(string line)
		{
			if (Regex.Match(line, @"set\s+[a-zA-Z_$][0-9a-zA-Z_]*\(", RegexOptions.IgnoreCase).Success) return true; else return false;
		}

		/// <summary>
		/// Checking if the line contains a class getter declaration
		/// </summary>
		/// <param name="line"></param>
		/// <returns>True if line contains class getter declaration else false</returns>
		internal static bool HasGetterDeclaration(string line)
		{
			if (Regex.Match(line, @"get\s+[a-zA-Z_$][0-9a-zA-Z_]*\(", RegexOptions.IgnoreCase).Success) return true; else return false;
		}

		/// <summary>
		/// Checking if the line contains a class constructor declaration
		/// </summary>
		/// <param name="line"></param>
		/// <returns>True if line contains class declaration else false</returns>
		internal static bool HasConstructorDeclaration(string line)
		{
			if (line.IndexOf("constructor(") == 0) return true; else return false;
		}

		/// <summary>
		/// Checking for predefined object names
		/// </summary>
		/// <param name="functionName"></param>
		/// <returns>true if the object is a predefined object else false</returns>
		internal static bool IsPredefinedObject(string objectName)
		{
			return objectName == "console" || objectName == "window";
		}

		/// <summary>
		/// Checking for predefined function over variables or objects
		/// </summary>
		/// <param name="functionName"></param>
		/// <returns>true if the funtion is a predefined funtions else false</returns>
		internal static bool IsPredefinedObjectFunction(string functionName)
		{
			return functionName == "hasOwnProperty" || functionName == "isPrototypeOf" || functionName == "propertyIsEnumerable" || functionName == "toLocaleString" || functionName == "toString" || functionName == "valueOf";
		}

		/// <summary>
		/// Checking for predefined function names
		/// </summary>
		/// <param name="functionName"></param>
		/// <returns>true if the funtion is a predefined funtions else false</returns>
		internal static bool IsPredefinedFunction(string functionName)
		{
			return functionName == "decodeURI" || functionName == "decodeURIComponent" || functionName == "encodeURI"
				|| functionName == "encodeURIComponent" || functionName == "escape" || functionName == "eval"
				|| functionName == "isFinite" || functionName == "isNaN" || functionName == "Number"
				|| functionName == "parseFloat" || functionName == "parseInt" || functionName == "String"
				|| functionName == "unescape";
		}
	}
}
