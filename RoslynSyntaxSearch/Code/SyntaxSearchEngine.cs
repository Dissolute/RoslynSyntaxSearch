using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;

namespace RoslynSyntaxSearch.Code
{
	public class SyntaxSearchEngine
	{

		private readonly InheritanceTree _typeTree;
		private SyntaxNodeCache _cache;

		public SyntaxSearchEngine(InheritanceTree typeTree)
		{
			_typeTree = typeTree;
		}

		private VisualStudioWorkspace _workspace;
		private VisualStudioWorkspace Workspace
		{
			get
			{
				if (_workspace == null)
				{

					var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
					_workspace = componentModel.GetService<VisualStudioWorkspace>();

				}

				return _workspace;
			}
		}

		public async System.Threading.Tasks.Task Refresh(CancellationToken ct)
		{

			if (ct.IsCancellationRequested) { return; }

			var solution = Workspace.CurrentSolution;

			var results = new List<SyntaxNode>();

			foreach (var project in solution.Projects)
			{
				var compilation = await project.GetCompilationAsync(ct);

				if (ct.IsCancellationRequested) { return; }

				foreach (var syntaxTree in compilation.SyntaxTrees)
				{
					var root = await syntaxTree.GetRootAsync(ct);

					if (ct.IsCancellationRequested) { return; }

					foreach (var node in root.DescendantNodesAndSelf(descendIntoTrivia: true))
					{
						results.Add(node);
					}
				}
			}

			_cache = new SyntaxNodeCache(_typeTree, results);

			return;
		}

		public async Task<IEnumerable<SyntaxNode>> GetNodesOfType(CancellationToken ct, Type type)
		{
			if (ct.IsCancellationRequested) { return null; }

			if (_cache == null)
			{
				await Refresh(ct);
			}

			if (ct.IsCancellationRequested) { return null; }

			return _cache.GetSyntaxNodesOfType(type);
		}

		public void NavigateToNode(SyntaxNode syntaxNode)
		{
			var solution = Workspace.CurrentSolution;

			var docId = solution.GetDocumentId(syntaxNode.SyntaxTree);

			if (docId == null)
			{
				return;
			}

			Workspace.OpenDocument(docId);

			var dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));

			var span = syntaxNode.GetLocation().GetLineSpan();

			var selection = dte.ActiveDocument.Selection as EnvDTE.TextSelection;

			// DTE LinePositions are 1-based (!)
			selection.MoveToLineAndOffset(span.StartLinePosition.Line + 1, span.StartLinePosition.Character + 1);
			selection.MoveToLineAndOffset(span.EndLinePosition.Line + 1, span.EndLinePosition.Character + 1, Extend: true);
		}

		public int GetTypeCount(Type nodeType) => _cache.GetTypeCount(nodeType);
	}
}
