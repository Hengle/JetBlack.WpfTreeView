using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.WpfTreeView
{
    [DebuggerDisplay("Value = {Value}, IsChecked = {IsChecked}, Parent = {Parent}, Children = {Children}")]
    public abstract class TreeViewModel : INotifyPropertyChanged
    {
        protected static readonly TreeViewModel DummyChild = new DummyTreeViewModel();

        private readonly Func<object, IEnumerable<LoaderResult>> _lazyLoader;
        private readonly IList<TreeViewModel> _children;
        private readonly Func<TreeViewModel, object, Func<object, IEnumerable<LoaderResult>>, TreeViewModel> _childFactory;

        private readonly AutoResetEvent _loadingEvent = new AutoResetEvent(true);

        protected TreeViewModel(TreeViewModel parent, object value, Func<object, IEnumerable<LoaderResult>> lazyLoader, Func<TreeViewModel, object, Func<object, IEnumerable<LoaderResult>>, TreeViewModel> childFactory)
        {
            Parent = parent;
            Value = value;
            _lazyLoader = lazyLoader;
            _childFactory = childFactory;

            _children = new ObservableCollection<TreeViewModel>();
            if (lazyLoader != null)
                _children.Add(DummyChild);
        }

        public TreeViewModel Parent { get; private set; }
        public object Value { get; private set; }
        public virtual IEnumerable<TreeViewModel> Children
        {
            get
            {
                if (_children.Contains(DummyChild) && _isExpanded)
                    GetChildren(children => OnPropertyChanged("Children"));

                return _children;
            }
        }

        public TreeViewModel Add(object item, Func<object, IEnumerable<LoaderResult>> lazyLoader = null)
        {
            var child = _childFactory(this, item, lazyLoader);
            Add(child);
            return child;
        }

        protected void Add(IEnumerable<TreeViewModel> children)
        {
            foreach (var child in children)
                Add(child);
        }

        virtual protected void Add(TreeViewModel child)
        {
            if (IsChecked.HasValue)
                child._isChecked = IsChecked;
            if (IsVisible.HasValue)
                child._isVisible = IsVisible;
            child.Parent = this;
            _children.Add(child);
        }

        private void AddChildren(IEnumerable<LoaderResult> children)
        {
            foreach (var child in children.Select(x => _childFactory(this, x.Child, x.LazyLoader)))
                Add(child);
        }

        #region Children

        public void GetChildren(Action<IList<TreeViewModel>> callback)
        {
            _loadingEvent.WaitOne();

            if (_children.Contains(DummyChild))
            {
                _children.Remove(DummyChild);

                IsLoading = true;

                Task.Factory.StartNew(() => _lazyLoader(Value))
                    .ContinueWith(x =>
                    {
                        AddChildren(x.Result);
                        IsLoading = false;
                        _loadingEvent.Set();
                        callback(_children);
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                _loadingEvent.Set();
                callback(_children);
            }
        }

        public IList<TreeViewModel> GetChildren()
        {
            _loadingEvent.WaitOne();

            if (_children.Contains(DummyChild))
            {
                _children.Remove(DummyChild);
                IsLoading = true;
                AddChildren(_lazyLoader(Value));
                IsLoading = false;
            }

            _loadingEvent.Set();

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

            OnPropertyChanged("Children");
            OnPropertyChanged("Items");
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
                {
                    OnPropertyChanged("Children");
                    OnPropertyChanged("Items");
                }
                OnPropertyChanged("IsExpanded");
            }
        }

        public void UpdateExpandedChildren()
        {
            UpdateChildren(x => x._isExpanded, (x, y) => x._isExpanded = y, _isExpanded, x => x.OnPropertyChanged("IsExpanded"));
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
            protected set { _isLoading = value; OnPropertyChanged("IsLoading"); }
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

                OnPropertyChanged("IsChecked");
            }
        }

        public event EventHandler<EventArgs> CheckedChanged;

        private void RaiseSelectedChanged(TreeViewModel value)
        {
            var handler = CheckedChanged;
            if (handler != null)
                handler(value, EventArgs.Empty);
        }

        private void UpdateSelectedChildren()
        {
            UpdateChildren(x => x._isChecked, (x, y) => x._isChecked = y, _isChecked, x => x.OnPropertyChanged("IsChecked"));
        }

        private void UpdateSelectedSiblingsAndParent(TreeViewModel sender, TreeViewModel child)
        {
            UpdateSiblingsAndParent(child, x => x._isChecked, (x, y) => x._isChecked = y, _isChecked, x => x.OnPropertyChanged("IsChecked"), x => x.RaiseSelectedChanged(sender));
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
                OnPropertyChanged("IsSelected");
            }
        }

        #endregion

        #region IsEnabled

        private bool? _isEnabled;

        public bool? IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; OnPropertyChanged("IsEnabled"); }
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

                OnPropertyChanged("IsVisible");
            }
        }

        private void UpdateVisibleChildren()
        {
            UpdateChildren(x => x._isVisible, (x, y) => x._isVisible = y, _isVisible, x => x.OnPropertyChanged("IsVisible"));
        }

        private void UpdateVisibleSiblingsAndParent(TreeViewModel child)
        {
            UpdateSiblingsAndParent(child, x => x._isVisible, (x, y) => x._isVisible = y, _isVisible, x => x.OnPropertyChanged("IsVisible"), x => { });
        }

        #endregion

        #region Node state maintenance

        protected void UpdateChildren(Func<TreeViewModel, bool?> getter, Action<TreeViewModel, bool?> setter, bool? value, Action<TreeViewModel> notify)
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

        protected void UpdateChildren(Func<TreeViewModel, bool> getter, Action<TreeViewModel, bool> setter, bool value, Action<TreeViewModel> notify)
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

        protected void UpdateSiblingsAndParent(TreeViewModel child, Func<TreeViewModel, bool?> getter, Action<TreeViewModel, bool?> setter, bool? value, Action<TreeViewModel> notify, Action<TreeViewModel> notifySender)
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

        private class DummyTreeViewModel : TreeViewModel
        {
            public DummyTreeViewModel()
                : base(null, null, null, null)
            {
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
