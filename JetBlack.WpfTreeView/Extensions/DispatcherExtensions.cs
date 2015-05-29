using System;
using System.Windows.Threading;

namespace JetBlack.WpfTreeView.Extensions
{
    public static class DispatcherExtensions
    {
        public static void InvokeOnDispacher(this Dispatcher dispatcher, DispatcherPriority dispatcherPriority, Action action)
        {
            if (dispatcher == null || dispatcher.CheckAccess())
                action();
            else
                dispatcher.Invoke(dispatcherPriority, action);
        }

        public static void InvokeOnDispacher(this Dispatcher dispatcher, Action action)
        {
            InvokeOnDispacher(dispatcher, DispatcherPriority.Normal, action);
        }

        public static void InvokeOnDispacher<T1>(this Dispatcher dispatcher, Action<T1> action, T1 arg1)
        {
            InvokeOnDispacher(dispatcher, DispatcherPriority.Normal, action, arg1);
        }

        public static void InvokeOnDispacher<T1, T2>(this Dispatcher dispatcher, Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            InvokeOnDispacher(dispatcher, DispatcherPriority.Normal, action, arg1, arg2);
        }

        public static void InvokeOnDispacher<T1>(this Dispatcher dispatcher, DispatcherPriority dispatcherPriority, Action<T1> action, T1 arg1)
        {
            if (dispatcher == null || dispatcher.CheckAccess())
                action(arg1);
            else
                dispatcher.Invoke(dispatcherPriority, action, arg1);
        }

        public static void InvokeOnDispacher<T1, T2>(this Dispatcher dispatcher, DispatcherPriority dispatcherPriority, Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            if (dispatcher == null || dispatcher.CheckAccess())
                action(arg1, arg2);
            else
                dispatcher.Invoke(dispatcherPriority, action, arg1, arg2);
        }

        public static void BeginInvokeOnDispacher(this Dispatcher dispatcher, DispatcherPriority dispatcherPriority, Action action)
        {
            if (dispatcher == null || dispatcher.CheckAccess())
                action();
            else
                dispatcher.BeginInvoke(dispatcherPriority, action);
        }

        public static void BeginInvokeOnDispacher(this Dispatcher dispatcher, Action action)
        {
            BeginInvokeOnDispacher(dispatcher, DispatcherPriority.Normal, action);
        }

        public static void BeginInvokeOnDispacher<T1>(this Dispatcher dispatcher, DispatcherPriority dispatcherPriority, Action<T1> action, T1 arg1)
        {
            if (dispatcher == null || dispatcher.CheckAccess())
                action(arg1);
            else
                dispatcher.BeginInvoke(dispatcherPriority, action, arg1);
        }

        public static void BeginInvokeOnDispacher<T1, T2>(this Dispatcher dispatcher, DispatcherPriority dispatcherPriority, Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            if (dispatcher == null || dispatcher.CheckAccess())
                action(arg1, arg2);
            else
                dispatcher.BeginInvoke(dispatcherPriority, action, arg1, arg2);
        }

        public static void BeginInvokeOnDispacher<T1, T2, T3>(this Dispatcher dispatcher, DispatcherPriority dispatcherPriority, Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            if (dispatcher == null || dispatcher.CheckAccess())
                action(arg1, arg2, arg3);
            else
                dispatcher.BeginInvoke(dispatcherPriority, action, arg1, arg2, arg3);
        }

        public static void BeginInvokeOnDispacher<T1>(this Dispatcher dispatcher, Action<T1> action, T1 arg1)
        {
            BeginInvokeOnDispacher(dispatcher, DispatcherPriority.Normal, action, arg1);
        }

        public static void BeginInvokeOnDispacher<T1, T2>(this Dispatcher dispatcher, Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            BeginInvokeOnDispacher(dispatcher, DispatcherPriority.Normal, action, arg1, arg2);
        }

        public static void BeginInvokeOnDispacher<T1, T2, T3>(this Dispatcher dispatcher, Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            BeginInvokeOnDispacher(dispatcher, DispatcherPriority.Normal, action, arg1, arg2, arg3);
        }
    }

}
