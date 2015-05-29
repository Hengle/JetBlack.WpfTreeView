using System;
using System.Collections.Generic;
using System.IO;
using JetBlack.Examples.TreeViewTest.Data;
using JetBlack.WpfTreeView.Models;
using JetBlack.WpfTreeView.ViewModels;

namespace JetBlack.Examples.TreeViewTest.ViewModels
{
    public class FileSystemTreeViewItemViewModel : TreeViewItemViewModel
    {
        public FileSystemTreeViewItemViewModel(DirectoryInfo folder)
            : this(null, folder, _ => FileSystemEnumerator.EnumerateChildren(folder))
        {
        }

        protected FileSystemTreeViewItemViewModel(TreeViewItemViewModel parent, object value, Func<object, IEnumerable<LoaderResult>> lazyLoader)
            : base(parent, value, lazyLoader, (p, v, l) => new FileSystemTreeViewItemViewModel(p, v, l))
        {
        }
    }
}
