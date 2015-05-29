namespace JetBlack.WpfTreeView.ViewModels
{
    public class CheckedChangedEventArgs
    {
        public CheckedChangedEventArgs(object value, bool? isChecked)
        {
            Value = value;
            IsChecked = isChecked;
        }

        public object Value { get; private set; }
        public bool? IsChecked { get; private set; }
    }
}
