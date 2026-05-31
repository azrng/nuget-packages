using JSqlParser.Net.Expression.Operators.Conditional;
using JSqlParser.Net.Expression.Operators.Relational;

namespace JSqlParser.Net.Expression.Cnf;

/// <summary>
/// Converts expressions to Conjunctive Normal Form (CNF).
/// CNF: (A OR B) AND (C OR D) AND ...
/// </summary>
public static class CNFConverter
{
    /// <summary>
    /// Convert an expression to CNF.
    /// </summary>
    public static Expression ConvertToCNF(Expression expression)
    {
        // Step 1: Push NOT down to leaves
        var pushed = PushNotDown(expression);
        // Step 2: Distribute OR over AND
        var distributed = DistributeOrOverAnd(pushed);
        // Step 3: Flatten
        return Flatten(distributed);
    }

    /// <summary>
    /// Push NOT down to leaves using De Morgan's laws.
    /// NOT (A AND B) → (NOT A) OR (NOT B)
    /// NOT (A OR B) → (NOT A) AND (NOT B)
    /// NOT NOT A → A
    /// </summary>
    private static Expression PushNotDown(Expression expr)
    {
        if (expr is NotExpression notExpr)
        {
            var inner = notExpr.Expression;
            if (inner is AndExpression andExpr)
            {
                // De Morgan: NOT (A AND B) → (NOT A) OR (NOT B)
                var newOr = new OrExpression();
                newOr.LeftExpression = PushNotDown(new NotExpression { Expression = andExpr.LeftExpression });
                newOr.RightExpression = PushNotDown(new NotExpression { Expression = andExpr.RightExpression });
                return PushNotDown(newOr);
            }
            if (inner is OrExpression orExpr)
            {
                // De Morgan: NOT (A OR B) → (NOT A) AND (NOT B)
                var newAnd = new AndExpression();
                newAnd.LeftExpression = PushNotDown(new NotExpression { Expression = orExpr.LeftExpression });
                newAnd.RightExpression = PushNotDown(new NotExpression { Expression = orExpr.RightExpression });
                return PushNotDown(newAnd);
            }
            if (inner is NotExpression innerNot)
            {
                // Double negation: NOT NOT A → A
                return PushNotDown(innerNot.Expression);
            }
            // NOT leaf - keep as is
            return notExpr;
        }
        if (expr is AndExpression and)
        {
            var result = new AndExpression();
            result.LeftExpression = PushNotDown(and.LeftExpression);
            result.RightExpression = PushNotDown(and.RightExpression);
            return result;
        }
        if (expr is OrExpression or)
        {
            var result = new OrExpression();
            result.LeftExpression = PushNotDown(or.LeftExpression);
            result.RightExpression = PushNotDown(or.RightExpression);
            return result;
        }
        // Leaf expression
        return expr;
    }

    /// <summary>
    /// Distribute OR over AND to achieve CNF.
    /// A OR (B AND C) → (A OR B) AND (A OR C)
    /// (B AND C) OR A → (B OR A) AND (C OR A)
    /// </summary>
    private static Expression DistributeOrOverAnd(Expression expr)
    {
        if (expr is OrExpression orExpr)
        {
            var left = DistributeOrOverAnd(orExpr.LeftExpression);
            var right = DistributeOrOverAnd(orExpr.RightExpression);

            if (left is AndExpression leftAnd)
            {
                // (A AND B) OR C → (A OR C) AND (B OR C)
                var newAnd = new AndExpression();
                newAnd.LeftExpression = DistributeOrOverAnd(new OrExpression { LeftExpression = leftAnd.LeftExpression, RightExpression = right });
                newAnd.RightExpression = DistributeOrOverAnd(new OrExpression { LeftExpression = leftAnd.RightExpression, RightExpression = right });
                return newAnd;
            }
            if (right is AndExpression rightAnd)
            {
                // A OR (B AND C) → (A OR B) AND (A OR C)
                var newAnd = new AndExpression();
                newAnd.LeftExpression = DistributeOrOverAnd(new OrExpression { LeftExpression = left, RightExpression = rightAnd.LeftExpression });
                newAnd.RightExpression = DistributeOrOverAnd(new OrExpression { LeftExpression = left, RightExpression = rightAnd.RightExpression });
                return newAnd;
            }
            // Both sides are not AND - keep as OR
            var newOr = new OrExpression();
            newOr.LeftExpression = left;
            newOr.RightExpression = right;
            return newOr;
        }
        if (expr is AndExpression andExpr)
        {
            var result = new AndExpression();
            result.LeftExpression = DistributeOrOverAnd(andExpr.LeftExpression);
            result.RightExpression = DistributeOrOverAnd(andExpr.RightExpression);
            return result;
        }
        // Leaf
        return expr;
    }

    /// <summary>
    /// Flatten nested AND expressions into MultiAndExpression.
    /// </summary>
    private static Expression Flatten(Expression expr)
    {
        if (expr is AndExpression andExpr)
        {
            var left = Flatten(andExpr.LeftExpression);
            var right = Flatten(andExpr.RightExpression);

            var result = new MultiAndExpression();
            CollectAndTerms(left, result.Expressions);
            CollectAndTerms(right, result.Expressions);
            return result;
        }
        return expr;
    }

    private static void CollectAndTerms(Expression expr, List<Expression> terms)
    {
        if (expr is MultiAndExpression multiAnd)
        {
            terms.AddRange(multiAnd.Expressions);
        }
        else if (expr is AndExpression andExpr)
        {
            CollectAndTerms(andExpr.LeftExpression, terms);
            CollectAndTerms(andExpr.RightExpression, terms);
        }
        else
        {
            terms.Add(expr);
        }
    }
}
