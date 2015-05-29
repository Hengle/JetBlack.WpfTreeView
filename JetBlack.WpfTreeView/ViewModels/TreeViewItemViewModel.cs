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
    public abstract class TreeViewItemViewModel : BindableBase
    {
        protected static readonly TreeViewItemViewModel DummyChild = new DummyTreeViewItemViewModel();

        private readonly Func<object, IEnumerable<LoaderResult>> _lazyLoader;
        private ObservableCollection<TreeViewItemViewModel> _children;
        private readonly Func<TreeViewItemViewModel, object, Func<object, IEnumerable<LoaderResult>>, TreeViewItemViewModel> _childFactory;

        protected TreeViewItemViewModel(TreeViewItemViewModel parent, object value, Func<object, IEnumerable<LoaderResult>> lazyLoader, Func<TreeViewItemViewModel, object, Func<object, IEnumerable<LoaderResult>>, TreeViewItemViewModel> childFactory)
        {
            Parent = parent;
            Value = value;
            _lazyLoader = lazyLoader;
            _childFactory = childFactory;

            _children = new ObservableCollection<TreeViewItemViewModel>();
            if (lazyLoader != null)
                _children.Add(DummyChild);
        }

        public TreeViewItemViewModel Parent { get; private set; }

        public object Value { get; private set; }

        public ObservableCollection<TreeViewItemViewModel> Children
        {
            get
            {
                if (_children.Contains(DummyChild) && _isExpanded)
                    GetChildren(children => OnPropertyChanged(() => Children));

                return _children;
            }
            private set
            {
                _children = value;
                OnPropertyChanged(() => Children);
            }
        }

        private void Add(IEnumerable<TreeViewItemViewModel> children)
        {
            Children = new ObservableCollection<TreeViewItemViewModel>(children.Select(InitialiseChild));
        }

        protected void Add(TreeViewItemViewModel child)
        {
            _children.Add(InitialiseChild(child));
        }

        private TreeViewItemViewModel InitialiseChild(TreeViewItemViewModel child)
        {
            if (IsChecked.HasValue)
                child._isChecked = IsChecked;
            if (IsVisible.HasValue)
                child._isVisible = IsVisible;
            child.Parent = this;
            return child;
        }

        private void AddChildren(IEnumerable<LoaderResult> children)
        {
            Add(children.Select(x => _childFactory(this, x.Child, x.LazyLoader)));
        }

        #region Children

        public void GetChildren(Action<ObservableCollection<TreeViewItemViewModel>> callback)
        {
            if (!_children.Contains(DummyChild))
                callback(_children);
            else
            {
                _children.Remove(DummyChild);

                IsLoading = true;

                Task.Factory.StartNew(() => _lazyLoader(Value))
                    .ContinueWith(x =>
                    {
                        AddChildren(x.Result);
                        IsLoading = false;
                        callback(_children);
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public ObservableCollection<TreeViewItemViewModel> GetChildren()
        {
            if (_children.Contains(DummyChild))
            {
                _children.Remove(DummyChild);
                IsLoading = true;
                AddChildren(_lazyLoader(Value));
                IsLoading = false;
            }

            return _children;
        }

        #endregion

        public void Reset()
        {
            if (_lazyLoader != null)
            {
                _children.Clear();
                _children.Add(DummyChild);
            }

            OnPropertyChanged(() => Children);
        }

        #region IsExpanded

        private bool _isExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                if (_children.Contains(DummyChild))
                    OnPropertyChanged(() => Children);
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

                UpdateSelectedChildren();

                if (Parent != null)
                    Parent.UpdateSelectedSiblingsAndParent(this, this);
                else
                    RaiseSelectedChanged(this);

                OnPropertyChanged(() => IsChecked);
            }
        }

        public event EventHandler<EventArgs> CheckedChanged;

        private void RaiseSelectedChanged(TreeViewItemViewModel value)
        {
            var handler = CheckedChanged;
            if (handler != null)
                handler(value, EventArgs.Empty);
        }

        private void UpdateSelectedChildren()
        {
            UpdateChildren(x => x._isChecked, (x, y) => x._isChecked = y, _isChecked, x => x.OnPropertyChanged(() => IsChecked));
        }

        private void UpdateSelectedSiblingsAndParent(TreeViewItemViewModel sender, TreeViewItemViewModel child)
        {
            UpdateSiblingsAndParent(child, x => x._isChecked, (x, y) => x._isChecked = y, _isChecked, x => x.OnPropertyChanged(() => IsChecked), x => x.RaiseSelectedChanged(sender));
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
            if (!value.HasValue || _children.Contains(DummyChild)) return;

            // Find children with a different selection state from ourself.
            foreach (var child in _children.Where(child => getter(child) != value))
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
            if (_children.Contains(DummyChild)) return;

            // Find children with a different selection state from ourself.
            foreach (var child in _children.Where(child => getter(child) != value))
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
                    (getter(child).HasValue && _children.All(x => getter(x) == getter(child)))
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
