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

		private SyntaxSearchResultViewModel _selectedResult;
		public SyntaxSearchResultViewModel SelectedResult
		{
			get => _selectedResult; set
			{
				if (_selectedResult != value)
				{
					_selectedResult = value;
					OnSelectedResultChanged();
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

		private void OnSelectedResultChanged()
		{
			SyntaxSearchEngine.Instance.NavigateToNode(SelectedResult.SyntaxNode);
		}

		public class SyntaxSearchResultViewModel
		{
			public SyntaxNode SyntaxNode { get; }

			public SyntaxSearchResultViewModel(SyntaxNode syntaxNode)
			{
				this.SyntaxNode = syntaxNode;
			}
			public string FileName => SyntaxNode.GetLocation().GetLineSpan().Path;

			public string SyntaxString => SyntaxNode.GetText().ToString();
		}
	}
}
