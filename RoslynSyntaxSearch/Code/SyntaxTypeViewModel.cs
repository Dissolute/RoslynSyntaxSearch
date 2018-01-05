using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynSyntaxSearch.Code
{
	public class SyntaxTypeViewModel
	{
		public SyntaxTypeViewModel(int inheritanceDepth, Type type)
		{
			InheritanceDepth = inheritanceDepth;
			Type = type;
		}

		public int InheritanceDepth { get; }
		public Type Type { get; }

		public string Name => $"{GetSpaces(InheritanceDepth * 2)} {Type.Name}";

		private static string GetSpaces(int numberOfSpaces)
		{
			//const string spaces = "                                                                 ";
			//const string spaces = "--->--->--->--->--->--->--->--->--->--->";
			const string spaces = " > > > > > > > > > > > > > >";
			return spaces.Substring(0, numberOfSpaces);
		}
	}
}
