using System;
using System.Linq.Expressions;

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
            _exp = _exp != null
                ? Expression.Lambda<Func<T, bool>>(Expression.AndAlso(_exp.Body, exp.Body),
                    _exp.Parameters)
                : exp;
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
            _exp = _exp != null
                ? Expression.Lambda<Func<T, bool>>(Expression.OrElse(_exp.Body, exp.Body),
                    _exp.Parameters)
                : exp;
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