using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBlack.WpfTreeView.Models;
using Microsoft.Practices.Prism.Mvvm;

namespace JetBlack.WpfTreeView.ViewModels
{
    [DebuggerDisplay("Value = {Value}, IsChecked = {IsChecked}, Parent = {Parent}, Children = {_children}")]
    public class TreeViewItemViewModel : BindableBase
    {
        private static readonly TreeViewItemViewModel DummyItem = new DummyTreeViewItemViewModel();

        private readonly Func<object, IEnumerable<LoaderResult>> _lazyLoader;
        private ObservableCollection<TreeViewItemViewModel> _items;
        private readonly Func<TreeViewItemViewModel, object, Func<object, IEnumerable<LoaderResult>>, TreeViewItemViewModel> _itemFactory;

        public TreeViewItemViewModel(TreeViewItemViewModel parent, object value, Func<object, IEnumerable<LoaderResult>> lazyLoader, Func<TreeViewItemViewModel, object, Func<object, IEnumerable<LoaderResult>>, TreeViewItemViewModel> itemFactory)
        {
            Parent = parent;
            Value = value;
            _lazyLoader = lazyLoader;
            _itemFactory = itemFactory;

            _items = new ObservableCollection<TreeViewItemViewModel>();
            if (lazyLoader != null)
                _items.Add(DummyItem);
        }

        public TreeViewItemViewModel Parent { get; private set; }

        private object _value;

        public object Value
        {
            get { return _value; }
            set { _value = value; OnPropertyChanged(() => Value); }
        }

        public ObservableCollection<TreeViewItemViewModel> Items
        {
            get
            {
                if (_items.Contains(DummyItem) && _isExpanded)
                    GetItems(_ => OnPropertyChanged(() => Items));

                return _items;
            }
            private set
            {
                _items = value;
                OnPropertyChanged(() => Items);
            }
        }

        private void Add(IEnumerable<TreeViewItemViewModel> items)
        {
            Items = new ObservableCollection<TreeViewItemViewModel>(items.Select(InitialiseItem));
        }

        protected void Add(TreeViewItemViewModel item)
        {
            _items.Add(InitialiseItem(item));
        }

        private TreeViewItemViewModel InitialiseItem(TreeViewItemViewModel item)
        {
            if (IsChecked.HasValue)
                item._isChecked = IsChecked;

            if (IsVisible.HasValue)
                item._isVisible = IsVisible;

            item.Parent = this;

            return item;
        }

        private void Add(IEnumerable<LoaderResult> results)
        {
            Add(results.Select(x => _itemFactory(this, x.Value, x.LazyLoader)));
        }

        #region Children

        public void GetItems(Action<ObservableCollection<TreeViewItemViewModel>> callback)
        {
            if (!_items.Contains(DummyItem))
                callback(_items);
            else
            {
                _items.Remove(DummyItem);

                IsLoading = true;

                Task.Factory.StartNew(() => _lazyLoader(Value))
                    .ContinueWith(x =>
                    {
                        Add(x.Result);
                        IsLoading = false;
                        callback(_items);
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public ObservableCollection<TreeViewItemViewModel> GetItems()
        {
            if (_items.Contains(DummyItem))
            {
                _items.Remove(DummyItem);
                IsLoading = true;
                Add(_lazyLoader(Value));
                IsLoading = false;
            }

            return _items;
        }

        #endregion

        public void Reset()
        {
            if (_lazyLoader != null)
            {
                _items.Clear();
                _items.Add(DummyItem);
            }

            OnPropertyChanged(() => Items);
        }

        #region IsExpanded

        private bool _isExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                if (_items.Contains(DummyItem))
                    OnPropertyChanged(() => Items);
                OnPropertyChanged(() => IsExpanded);
            }
        }

        public void UpdateExpandedChildren()
        {
            UpdateChildren(x => x._isExpanded, (x, y) => x._isExpanded = y, _isExpanded, x => x.OnPropertyChanged(() => IsExpanded));
        }

        public void ExpandParentNodes()
        {
            if (Parent == null) return;
            Parent.IsExpanded = true;
            Parent.ExpandParentNodes();
        }

        #endregion

        #region IsLoading

        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            protected set { _isLoading = value; OnPropertyChanged(() => IsLoading); }
        }

        #endregion

        #region IsChecked

        private bool? _isChecked = false;

        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                // The null state is implied by the state of the children, and cannot be set directly.
                if (!value.HasValue)
                    throw new ArgumentException(@"Cannot set to null directly", "value");

                // Do nothing if the child is unchanged.
                if (_isChecked == value)
                    return;

                _isChecked = value;

                UpdateCheckedChildren();

                if (Parent != null)
                    Parent.UpdateCheckedSiblingsAndParent(this, this);
                else
                    RaiseCheckedChanged(this);

                OnPropertyChanged(() => IsChecked);
            }
        }

        public event EventHandler<CheckedChangedEventArgs> CheckedChanged;

        private void RaiseCheckedChanged(TreeViewItemViewModel item)
        {
            var handler = CheckedChanged;
            if (handler != null)
                handler(item, new CheckedChangedEventArgs(item.Value, item.IsChecked));
        }

        private void UpdateCheckedChildren()
        {
            UpdateChildren(x => x._isChecked, (x, y) => x._isChecked = y, _isChecked, x => x.OnPropertyChanged(() => IsChecked));
        }

        private void UpdateCheckedSiblingsAndParent(TreeViewItemViewModel sender, TreeViewItemViewModel child)
        {
            UpdateSiblingsAndParent(child, x => x._isChecked, (x, y) => x._isChecked = y, _isChecked, x => x.OnPropertyChanged(() => IsChecked), x => x.RaiseCheckedChanged(sender));
        }

        #endregion

        #region IsSelected

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(() => IsSelected);
            }
        }

        #endregion

        #region IsEnabled

        private bool? _isEnabled;

        public bool? IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; OnPropertyChanged(() => IsEnabled); }
        }

        #endregion

        #region IsVisible

        private bool? _isVisible = true;

        public bool? IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value) return;

                _isVisible = value;

                UpdateVisibleChildren();

                if (Parent != null)
                    Parent.UpdateVisibleSiblingsAndParent(this);

                OnPropertyChanged(() => IsVisible);
            }
        }

        private void UpdateVisibleChildren()
        {
            UpdateChildren(x => x._isVisible, (x, y) => x._isVisible = y, _isVisible, x => x.OnPropertyChanged(() => IsVisible));
        }

        private void UpdateVisibleSiblingsAndParent(TreeViewItemViewModel child)
        {
            UpdateSiblingsAndParent(child, x => x._isVisible, (x, y) => x._isVisible = y, _isVisible, x => x.OnPropertyChanged(() => IsVisible), x => { });
        }

        #endregion

        #region Node state maintenance

        protected void UpdateChildren(Func<TreeViewItemViewModel, bool?> getter, Action<TreeViewItemViewModel, bool?> setter, bool? value, Action<TreeViewItemViewModel> notify)
        {
            // If we haven't dot a definate selection state, or we haven't yet loaded the children, go no further.
            if (!value.HasValue || _items.Contains(DummyItem)) return;

            // Find children with a different selection state from ourself.
            foreach (var child in _items.Where(child => getter(child) != value))
            {
                // Set the child state and raise the event.
                setter(child, value);
                notify(child);

                // Update the childs children.
                child.UpdateChildren(getter, setter, value, notify);
            }
        }

        protected void UpdateChildren(Func<TreeViewItemViewModel, bool> getter, Action<TreeViewItemViewModel, bool> setter, bool value, Action<TreeViewItemViewModel> notify)
        {
            // If we haven't dot a definate selection state, or we haven't yet loaded the children, go no further.
            if (_items.Contains(DummyItem)) return;

            // Find children with a different selection state from ourself.
            foreach (var child in _items.Where(child => getter(child) != value))
            {
                // Set the child state and raise the event.
                setter(child, value);
                notify(child);

                // Update the childs children.
                child.UpdateChildren(getter, setter, value, notify);
            }
        }

        protected void UpdateSiblingsAndParent(TreeViewItemViewModel child, Func<TreeViewItemViewModel, bool?> getter, Action<TreeViewItemViewModel, bool?> setter, bool? value, Action<TreeViewItemViewModel> notify, Action<TreeViewItemViewModel> notifySender)
        {
            if (value != getter(child))
            {
                // If the child has a value and all children share the same value then that is our state, otherwise
                // the state is null.
                var state =
                    (getter(child).HasValue && _items.All(x => getter(x) == getter(child)))
                        ? getter(child)
                        : null;

                // Does this change the state of this node?
                if (value != state)
                {
                    // Set the new state and raise an event.
                    setter(this, state);
                    notify(this);
                }
            }

            if (Parent != null)
                Parent.UpdateSiblingsAndParent(this, getter, setter, value, notify, notifySender);
            else
                notifySender(this);
        }

        #endregion

        private class DummyTreeViewItemViewModel : TreeViewItemViewModel
        {
            public DummyTreeViewItemViewModel()
                : base(null, null, null, null)
            {
            }
        }
    }
}
