namespace Azrng.Core.Helpers
{
    /// <summary>
    /// Simple interface to provide a generic mechanism to build guard clause extension methods from.
    /// </summary>
    public interface IGuardClause { }

    /// <summary>
    /// An entry point to a set of Guard Clauses defined as extension methods on IGuardClause.
    /// </summary>
    /// <remarks>See https://github.com/ardalis/GuardClauses </remarks>
    public class Guard : IGuardClause
    {
        /// <summary>
        /// 守卫的入口
        /// </summary>
        public static IGuardClause Against { get; } = new Guard();

        private Guard() { }
    }
}