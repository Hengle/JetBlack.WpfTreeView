using System;
using System.Collections.Generic;

namespace JetBlack.WpfTreeView.Models
{
    public class LoaderResult
    {
        public LoaderResult(object value, Func<object, IEnumerable<LoaderResult>> lazyLoader = null)
        {
            Value = value;
            LazyLoader = lazyLoader;
        }

        public object Value { get; private set; }
        public Func<object, IEnumerable<LoaderResult>> LazyLoader { get; private set; }
    }
}
