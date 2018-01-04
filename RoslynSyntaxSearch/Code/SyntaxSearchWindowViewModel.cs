using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.CodeAnalysis;

namespace RoslynSyntaxSearch.Code
{
	public class SyntaxSearchWindowViewModel
	{
		private readonly SerialDisposable _searchSubscription = new SerialDisposable();
		public SyntaxSearchWindowViewModel()
		{
			var tree = new InheritanceTree(typeof(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode));
			var nodes = tree.GetTreeNodes();
			SyntaxTypes = nodes.Select(n => new SyntaxTypeViewModel(n.depth, n.type)).ToArray();
		}
		public SyntaxTypeViewModel[] SyntaxTypes { get; }

		public ObservableCollection<SyntaxSearchResultViewModel> SearchResults { get; } = new ObservableCollection<SyntaxSearchResultViewModel>();

		private SyntaxTypeViewModel _selectedSyntax;
		public SyntaxTypeViewModel SelectedSyntax
		{
			get => _selectedSyntax;
			set
			{
				if (_selectedSyntax != value)
				{
					_selectedSyntax = value;
					UpdateSearchResults();
				}
			}
		}

		private void UpdateSearchResults()
		{
			var cd = new CancellationDisposable();
			_searchSubscription.Disposable = cd;
			Task.Run(() => UpdateSearchResults(cd.Token), cd.Token);
		}

		private async Task UpdateSearchResults(CancellationToken ct)
		{
			if (ct.IsCancellationRequested) { return; }
			await Application.Current.Dispatcher.InvokeAsync(() => SearchResults.Clear(), DispatcherPriority.Normal, ct);

			var results = await SyntaxSearchEngine.Instance.GetNodesOfType(ct, SelectedSyntax.Type);

			if (ct.IsCancellationRequested) { return; }

			await Application.Current.Dispatcher.InvokeAsync(() =>
			{
				foreach (var result in results)
				{
					SearchResults.Add(new SyntaxSearchResultViewModel(result));
				}
			},
				DispatcherPriority.Normal,
				ct
			);
		}

		public class SyntaxSearchResultViewModel
		{
			private readonly SyntaxNode syntaxNode;

			public SyntaxSearchResultViewModel(SyntaxNode syntaxNode)
			{
				this.syntaxNode = syntaxNode;
			}
			public string FileName => syntaxNode.GetLocation().GetLineSpan().Path;

			public string SyntaxString => syntaxNode.GetText().ToString();
		}
	}
}
