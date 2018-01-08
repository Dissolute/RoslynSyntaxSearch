using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RoslynSyntaxSearch.Code
{
	public class InheritanceTree
	{
		private readonly Dictionary<Type, List<Type>> _tree;
		private readonly Type _rootType;

		public InheritanceTree(Type rootType, Assembly assembly = null)
		{
			_rootType = rootType;
			assembly = assembly ?? rootType.GetTypeInfo().Assembly;

			var derivedTypes = assembly.GetTypes()
				.Where(t => rootType.IsAssignableFrom(t))
				.ToArray();

			_tree = derivedTypes.ToDictionary(t => t, t => new List<Type>(0));

			foreach (var type in derivedTypes)
			{
				if (type == rootType)
				{
					continue;
				}

				var baseType = type.GetTypeInfo().BaseType;
				_tree[baseType].Add(type);
			}
			;
		}

		public IEnumerable<(Type type, int depth)> GetTreeNodes() => GetTreeNodes(_rootType, 0);

		private IEnumerable<(Type type, int depth)> GetTreeNodes(Type baseType, int currentDepth)
		{
			yield return (baseType, currentDepth);

			foreach (var type in _tree[baseType])
			{
				foreach (var node in GetTreeNodes(type, currentDepth + 1))
				{
					yield return node;
				}
			}
		}

		public IEnumerable<Type> GetSubtree(Type rootType) => GetTreeNodes(rootType, 0).Select(tuple => tuple.type);
	}
}
