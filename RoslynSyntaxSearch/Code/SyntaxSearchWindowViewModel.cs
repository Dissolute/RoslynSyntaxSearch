using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynSyntaxSearch.Code
{
	public class SyntaxSearchWindowViewModel
	{
		public object SyntaxTypes => Enumerable.Range(0, 10);
		public object SearchResults => Enumerable.Range(0, 10).Select(_ => new SyntaxSearchResultViewModel());

		public class SyntaxSearchResultViewModel
		{
			public string FileName => "file";

			public string SyntaxString => "syntax";
		}
	}
}
