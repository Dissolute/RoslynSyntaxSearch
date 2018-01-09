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
	public class SyntaxSearchWindowViewModel : ViewModel
	{
		private readonly SerialDisposable _searchSubscription = new SerialDisposable();
		private readonly SyntaxSearchEngine _engine;

		public SyntaxSearchWindowViewModel()
		{
			var tree = new InheritanceTree(typeof(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode));
			var nodes = tree.GetTreeNodes();
			SyntaxTypes = nodes.Select(n => new SyntaxTypeViewModel(n.depth, n.type)).ToArray();

			_engine = new SyntaxSearchEngine(tree);
		}
		public SyntaxTypeViewModel[] SyntaxTypes { get; }

		private IList<SyntaxSearchResultViewModel> _searchResults;
		public IList<SyntaxSearchResultViewModel> SearchResults
		{
			get => _searchResults;
			private set => SetPropertyField(ref _searchResults, value);
		}

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

		private string _searchResultSummary = "";

		public string SearchResultSummary
		{
			get => _searchResultSummary;
			set
			{
				SetPropertyField(ref _searchResultSummary, value);
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

			SearchResults = null;

			SearchResultSummary = "Searching...";

			var results = await _engine.GetNodesOfType(ct, SelectedSyntax.Type);

			if (ct.IsCancellationRequested) { return; }

			UpdateResultsCounts();

			var resultsVM = new List<SyntaxSearchResultViewModel>();

			foreach (var result in results)
			{
				resultsVM.Add(new SyntaxSearchResultViewModel(result));
			}

			SearchResults = resultsVM;

			SearchResultSummary = $"{SearchResults.Count} result(s).";

		}

		private void UpdateResultsCounts()
		{
			foreach (var svm in SyntaxTypes)
			{
				svm.ResultCount = _engine.GetTypeCount(svm.Type);
			}
		}

		private void OnSelectedResultChanged()
		{
			_engine.NavigateToNode(SelectedResult.SyntaxNode);
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
