using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace RoslynSyntaxSearch.Code
{
	/// <summary>
	/// Cache the results of a type search on syntax nodes. 
	/// </summary>
	public class SyntaxNodeCache
	{
		private readonly IDictionary<Type, List<SyntaxNode>> _syntaxNodes;
		private readonly InheritanceTree _typeTree;

		public SyntaxNodeCache(InheritanceTree typeTree, IEnumerable<SyntaxNode> nodes)
		{
			_typeTree = typeTree;

			_syntaxNodes = _typeTree.GetTreeNodes().Select(n => n.type).ToDictionary(t => t, _ => new List<SyntaxNode>());

			foreach (var node in nodes)
			{
				_syntaxNodes[node.GetType()].Add(node);
			}
		}

		private IEnumerable<List<SyntaxNode>> GetAllNodeLists(Type nodeType)
		{
			foreach (var type in _typeTree.GetSubtree(nodeType))
			{
				yield return _syntaxNodes[type];
			}
		}

		public int GetTypeCount(Type nodeType)
		{
			return GetAllNodeLists(nodeType).Sum(l => l.Count);
		}

		public IEnumerable<SyntaxNode> GetSyntaxNodesOfType(Type nodeType)
		{
			return GetAllNodeLists(nodeType).SelectMany(l => l);
		}
	}
}
