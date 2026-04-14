using System;
using System.Linq.Expressions;
using Azrng.Core.CommonDto;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 表达式构建
    /// </summary>
    public class Expressionable
    {
        public static Expressionable<T> Create<T>() where T : class, new() => new Expressionable<T>();
    }

    public class Expressionable<T> where T : class, new()
    {
        private Expression<Func<T, bool>>? _exp;

        public Expressionable<T> And(Expression<Func<T, bool>> exp)
        {
            if (_exp == null)
            {
                _exp = exp;
                return this;
            }

            var parameter = _exp.Parameters[0];
            var visitor = new PredicateExpressionVisitor(parameter);
            var rightBody = visitor.Visit(exp.Body);
            _exp = Expression.Lambda<Func<T, bool>>(Expression.AndAlso(_exp.Body, rightBody), parameter);
            return this;
        }

        public Expressionable<T> AndIF(bool isAnd, Expression<Func<T, bool>> exp)
        {
            if (isAnd)
                And(exp);
            return this;
        }

        public Expressionable<T> Or(Expression<Func<T, bool>> exp)
        {
            if (_exp == null)
            {
                _exp = exp;
                return this;
            }

            var parameter = _exp.Parameters[0];
            var visitor = new PredicateExpressionVisitor(parameter);
            var rightBody = visitor.Visit(exp.Body);
            _exp = Expression.Lambda<Func<T, bool>>(Expression.OrElse(_exp.Body, rightBody), parameter);
            return this;
        }

        public Expressionable<T> OrIF(bool isOr, Expression<Func<T, bool>> exp)
        {
            if (isOr)
                Or(exp);
            return this;
        }

        public Expression<Func<T, bool>> ToExpression()
        {
            return _exp ??= it => true;
        }
    }
}
