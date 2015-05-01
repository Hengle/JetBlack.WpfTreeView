using System;
using System.Collections.Generic;

namespace JetBlack.WpfTreeView
{
    public class LoaderResult
    {
        public LoaderResult(object child, Func<object, IEnumerable<LoaderResult>> lazyLoader = null)
        {
            Child = child;
            LazyLoader = lazyLoader;
        }

        public object Child { get; set; }
        public Func<object, IEnumerable<LoaderResult>> LazyLoader { get; set; }
    }
}
