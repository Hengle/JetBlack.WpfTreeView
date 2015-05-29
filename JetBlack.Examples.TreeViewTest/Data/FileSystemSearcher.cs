using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.TreeViewTest.Data
{
    public static class FileSystemSearcher
    {
        public static IObservable<IList<Predicate<object>>> SearchObservable(string searchText)
        {
            return Observable.Create<IList<Predicate<object>>>(async (observer, token) =>
            {
                await Task.Factory.StartNew(() =>
                {
                    Search("C:\\", searchText, token)
                        .ToObservable()
                        .Subscribe(path =>
                            observer.OnNext(
                                path.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                                .Skip(1)
                                .Select(x => new Predicate<object>(y => ((FileSystemInfo)y).Name == x))
                                .ToList()));
                }, token);
            });
        }
        public static IEnumerable<string> Search(string root, string searchPattern, CancellationToken token)
        {
            var dirs = new Queue<string>();
            dirs.Enqueue(root);
            while (!token.IsCancellationRequested && dirs.Count > 0)
            {
                var dir = dirs.Dequeue();

                // files
                string[] paths = null;
                try
                {
                    paths = Directory.GetFiles(dir, searchPattern);
                }
                catch { } // swallow

                if (paths != null && paths.Length > 0)
                {
                    foreach (string file in paths)
                    {
                        if (token.IsCancellationRequested)
                            yield break;

                        yield return file;
                    }
                }

                if (token.IsCancellationRequested)
                    yield break;

                // sub-directories
                paths = null;
                try
                {
                    paths = Directory.GetDirectories(dir);
                }
                catch { } // swallow

                if (paths != null && paths.Length > 0)
                    foreach (var subDir in paths)
                        dirs.Enqueue(subDir);
            }
        }
    }
}
