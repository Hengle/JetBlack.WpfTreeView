using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace JetBlack.WpfTreeView.Extensions
{
    public struct PropertyChange
    {
        public PropertyChange(string propertyName, object value)
            : this()
        {
            PropertyName = propertyName;
            Value = value;
        }

        public string PropertyName { get; private set; }
        public object Value { get; private set; }

        public override string ToString()
        {
            return string.Format("PropertyName={0}, Value={1}", PropertyName, Value);
        }
    }

    public static class PropertyChangeExtensions
    {
        public static IObservable<PropertyChange> ToObservable<TSource>(this TSource viewModel)
            where TSource : INotifyPropertyChanged
        {
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                (h => viewModel.PropertyChanged += h),
                (h => viewModel.PropertyChanged -= h))
                .Select(x =>
                {
                    Expression expr = Expression.Property(Expression.Constant(viewModel), x.EventArgs.PropertyName);
                    var value = Expression.Lambda(expr).Compile().DynamicInvoke();
                    return new PropertyChange(x.EventArgs.PropertyName, value);
                });
        }

        public static IObservable<TProperty> ToObservable<TSource, TProperty>(this TSource viewModel, Expression<Func<TSource, TProperty>> propertyExpression)
            where TSource : INotifyPropertyChanged
        {
            var propertyName = GetPropertyName(propertyExpression);

            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                (h => viewModel.PropertyChanged += h),
                (h => viewModel.PropertyChanged -= h))
                .Where(x => x.EventArgs.PropertyName == propertyName)
                .Select(x => propertyExpression.Compile()(viewModel));
        }

        public static string GetPropertyName<TSource, TProperty>(Expression<Func<TSource, TProperty>> property)
        {
            var lambda = (LambdaExpression)property;

            var memberExpression =
                lambda.Body is UnaryExpression
                ? (MemberExpression)(((UnaryExpression)lambda.Body).Operand)
                : (MemberExpression)lambda.Body;

            return memberExpression.Member.Name;
        }
    }

}
