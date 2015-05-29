using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBlack.WpfTreeView.Models;

namespace JetBlack.Examples.TreeViewTest.Data
{
    public static class FileSystemEnumerator
    {
        public static IEnumerable<LoaderResult> EnumerateChildren(object parent)
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
