using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;

namespace RoslynSyntaxSearch.Code
{
	public class SyntaxSearchEngine
	{
		public static SyntaxSearchEngine Instance { get; } = new SyntaxSearchEngine();
		private SyntaxSearchEngine()
		{

		}

		private VisualStudioWorkspace _workspace;
		private VisualStudioWorkspace Workspace
		{
			get
			{
				if (_workspace == null)
				{

					var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
					_workspace = componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();

				}

				return _workspace;
			}
		}

		public async Task<IEnumerable<SyntaxNode>> GetNodesOfType(CancellationToken ct, Type type)
		{
			if (ct.IsCancellationRequested) { return null; }

			var solution = Workspace.CurrentSolution;

			var results = new List<SyntaxNode>();

			foreach (var project in solution.Projects)
			{
				var compilation = await project.GetCompilationAsync(ct);

				if (ct.IsCancellationRequested) { return results; }

				foreach (var syntaxTree in compilation.SyntaxTrees)
				{
					var root = await syntaxTree.GetRootAsync(ct);

					if (ct.IsCancellationRequested) { return results; }

					foreach (var node in root.DescendantNodesAndSelf(descendIntoTrivia: true))
					{
						if (type.IsAssignableFrom(node.GetType()))
						{
							results.Add(node);
						}
					}
				}
			}

			return results;
		}

	}
}
