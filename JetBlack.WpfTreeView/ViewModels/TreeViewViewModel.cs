using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using JetBlack.WpfTreeView.Extensions;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace JetBlack.WpfTreeView.ViewModels
{
    public class TreeViewViewModel : BindableBase
    {
        private readonly Func<string, IObservable<IList<Predicate<object>>>> _searchFunc;
        private readonly TreeViewItemViewModel _treeViewItemViewModel;
        private string _searchText;
        private CancellationTokenSource _searchTokenSource;

        public TreeViewViewModel(TreeViewItemViewModel treeViewItemViewModelViewModel, Func<string, IObservable<IList<Predicate<object>>>> searchFunc = null)
        {
            _searchFunc = searchFunc;
            ClearSearchCommand = new DelegateCommand(ClearSearch, CanSearch);
            _treeViewItemViewModel = treeViewItemViewModelViewModel;

            this.ToObservable(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(1000), DispatcherScheduler.Current)
                .Where(_ => CanSearch())
                .Subscribe(_ => Search());
        }

        public IEnumerable<TreeViewItemViewModel> Items
        {
            get { return _treeViewItemViewModel.GetItems(); }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(() => SearchText);
                ClearSearchCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand ClearSearchCommand { get; private set; }

        private void Search()
        {
            CancelSearch();

            _searchTokenSource = new CancellationTokenSource();
            var token = _searchTokenSource.Token;

            _treeViewItemViewModel.IsVisible = false;

            _searchFunc(SearchText)
                .ObserveOn(TaskPoolScheduler.Default)
                .Select(predicate => new Queue<Predicate<object>>(predicate))
                .Subscribe(
                    path => Dispatcher.BeginInvokeOnDispacher(DispatcherPriority.Background, MakeVisible, _treeViewItemViewModel, path, token),
                    token);
        }

        private static Dispatcher Dispatcher
        {
            get { return Application.Current.Dispatcher; }
        }

        private static void MakeVisible(TreeViewItemViewModel node, Queue<Predicate<object>> path, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            if (path.Count == 0)
                node.IsVisible = true;
            else
            {
                var item = path.Dequeue();

                node.GetItems(children =>
                {
                    if (token.IsCancellationRequested)
                        return;

                    var child = children.FirstOrDefault(x => item(x.Value));
                    if (child != null)
                    {
                        node.IsExpanded = true;
                        MakeVisible(child, path, token);
                    }
                });
            }
        }

        private bool CanSearch()
        {
            return !(_searchFunc == null || string.IsNullOrWhiteSpace(SearchText) || SearchText.Length < 3);
        }

        private void CancelSearch()
        {
            if (_searchTokenSource != null)
            {
                _searchTokenSource.Cancel();
                _searchTokenSource = null;
            }
        }

        private void ResetTree()
        {
            foreach (var child in _treeViewItemViewModel.Items)
            {
                child.IsExpanded = false;
                child.UpdateExpandedChildren();
            }

            _treeViewItemViewModel.IsVisible = true;
        }

        private void ClearSearch()
        {
            CancelSearch();
            ResetTree();
            SearchText = string.Empty;
        }
    }
}
