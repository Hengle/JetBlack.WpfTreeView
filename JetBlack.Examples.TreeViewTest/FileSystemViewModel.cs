using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBlack.WpfTreeView;

namespace JetBlack.Examples.TreeViewTest
{
    public class FileSystemViewModel : TreeViewModel
    {
        private IList<FileSystemViewModel> _root;

        public FileSystemViewModel(DirectoryInfo folder)
            : this(null, folder, _ => EnumerateChildren(folder))
        {
        }

        protected FileSystemViewModel(TreeViewModel parent, object value, Func<object, IEnumerable<LoaderResult>> lazyLoader)
            : base(parent, value, lazyLoader, Create)
        {
        }

        public FileSystemViewModel()
            : base(null, null, null, null)
        {
        }

        public IList<FileSystemViewModel> Root
        {
            get { return _root ?? (_root = new List<FileSystemViewModel> { this }); }
        }

        private static TreeViewModel Create(TreeViewModel parent, object value, Func<object, IEnumerable<LoaderResult>> lazyLoader)
        {
            return new FileSystemViewModel(parent, value, lazyLoader);
        }

        private static IEnumerable<LoaderResult> EnumerateChildren(object parent)
        {
            var results = new List<LoaderResult>();

            var directoryInfo = parent as DirectoryInfo;
            if (directoryInfo == null)
                return results;

            results.AddRange(
                directoryInfo.EnumerateDirectories()
                    .Select(child => new LoaderResult(child, EnumerateChildren)));

            results.AddRange(
                directoryInfo.EnumerateFiles()
                    .Select(file => new LoaderResult(file)));

            return results;
        }
    }
}
