using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AdTool.Core
{
    public static class ExpressionHelpers
    {
        public static T GetPropertyValue<T>(this Expression<Func<T>> lambda)
        {
            return lambda.Compile().Invoke();
        }

        public static void SetPropertyValue<T>(this Expression<Func<T>> lambda, T value)
        {
            var expression = (lambda as LambdaExpression).Body as MemberExpression;
            var propertyInfo = (PropertyInfo)expression.Member;
            var target = Expression.Lambda(expression.Expression).Compile().DynamicInvoke();

            propertyInfo.SetValue(target, value);
        }

    }
}
