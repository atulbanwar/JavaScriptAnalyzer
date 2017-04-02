﻿using static JavaScriptAnalyzer.Util.Globals;

namespace JavaScriptAnalyzer.POCO
{
	class Variable
	{
		public string Name { get; set; }
		public int LineNo { get; set; }
		public bool IsUsed { get; set; }
		public VariableType Type { get; set; }
		public string ObjectName { get; set; }
	}
}