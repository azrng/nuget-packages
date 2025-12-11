using System.Linq.Expressions;

namespace Azrng.Core.CommonDto
{
    /// <summary>
    /// 用于遍历并修改表达式树的访问器，主要用于替换参数表达式。
    /// </summary>
    public class PredicateExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        /// 要用于替换的参数表达式。
        /// </summary>
        private ParameterExpression Parameter { get; set; }

        /// <summary>
        /// 构造函数，初始化要替换的参数。
        /// </summary>
        /// <param name="parameter">用于替换的参数表达式</param>
        public PredicateExpressionVisitor(ParameterExpression parameter)
        {
            Parameter = parameter;
        }

        /// <summary>
        /// 重写 VisitParameter 方法，将所有参数表达式替换为指定的 Parameter。
        /// </summary>
        /// <param name="p">当前遍历到的参数表达式</param>
        /// <returns>替换后的参数表达式</returns>
        protected override Expression VisitParameter(ParameterExpression p)
        {
            return Parameter;
        }

        /// <summary>
        /// 重写 Visit 方法，递归遍历表达式树并进行参数替换。
        /// </summary>
        /// <param name="expression">要访问的表达式</param>
        /// <returns>处理后的表达式树</returns>
        public override Expression? Visit(Expression expression)
        {
            // 通过调用 base.Visit 方法，递归访问并处理表达式树中的各个节点
            // 其中 VisitParameter 方法会被调用，用于替换参数
            return base.Visit(expression);
        }
    }
}