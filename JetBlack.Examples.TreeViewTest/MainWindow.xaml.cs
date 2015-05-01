using System.ComponentModel;
using System.IO;

namespace JetBlack.Examples.TreeViewTest
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private FileSystemViewModel _fileSystemViewModel;

        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new FileSystemViewModel(new DirectoryInfo("C:\\"));
        }

        public FileSystemViewModel ViewModel
        {
            get { return _fileSystemViewModel; }
            set
            {
                _fileSystemViewModel = value;
                OnPropertyChanged("ViewModel");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
