using System.IO;
using JetBlack.Examples.TreeViewTest.Data;
using JetBlack.WpfTreeView.ViewModels;

namespace JetBlack.Examples.TreeViewTest.ViewModels
{
    public class FileSystemTreeViewViewModel : TreeViewViewModel
    {
        public FileSystemTreeViewViewModel()
            : base(new FileSystemTreeViewItemViewModel(new DirectoryInfo("C:\\")), FileSystemSearcher.SearchObservable)
        {
        }
    }
}
