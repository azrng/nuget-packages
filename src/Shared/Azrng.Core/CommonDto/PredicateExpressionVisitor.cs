using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Azrng.Core.CommonDto
{
    /// <summary>
    /// 用于遍历并修改表达式树的访问器，主要用于替换参数表达式。
    /// </summary>
    public class PredicateExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        /// 待替换的来源参数集合（引用相等匹配）。
        /// </summary>
        private readonly HashSet<ParameterExpression> _sources;

        /// <summary>
        /// 替换后的目标参数表达式。
        /// </summary>
        private readonly ParameterExpression _target;

        /// <summary>
        /// 构造函数，把单个来源参数替换为目标参数。
        /// </summary>
        /// <param name="source">待替换的来源参数表达式</param>
        /// <param name="target">替换后的目标参数表达式</param>
        public PredicateExpressionVisitor(ParameterExpression source, ParameterExpression target)
            : this([source], target)
        {
        }

        /// <summary>
        /// 构造函数，把多个来源参数替换为同一目标参数。
        /// </summary>
        /// <param name="sources">待替换的来源参数表达式集合</param>
        /// <param name="target">替换后的目标参数表达式</param>
        public PredicateExpressionVisitor(IEnumerable<ParameterExpression> sources, ParameterExpression target)
        {
            _sources = [.. sources];
            _target = target;
        }

        /// <summary>
        /// 重写 VisitParameter 方法，仅替换来源参数集合中的参数表达式。
        /// </summary>
        /// <param name="p">当前遍历到的参数表达式</param>
        /// <returns>替换后的参数表达式</returns>
        protected override Expression VisitParameter(ParameterExpression p)
        {
            //嵌套 lambda（如 Any/Where 内层）声明的参数保持原样，避免构建出非法表达式树
            return _sources.Contains(p) ? _target : p;
        }

        /// <summary>
        /// 重写 Visit 方法，递归遍历表达式树并进行参数替换。
        /// </summary>
        /// <param name="expression">要访问的表达式</param>
        /// <returns>处理后的表达式树</returns>
        public override Expression Visit(Expression? expression)
        {
            // 通过调用 base.Visit 方法，递归访问并处理表达式树中的各个节点
            // 其中 VisitParameter 方法会被调用，用于替换参数
            return base.Visit(expression) ?? throw new InvalidOperationException();
        }
    }
}