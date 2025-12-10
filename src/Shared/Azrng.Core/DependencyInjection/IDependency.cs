namespace Azrng.Core.DependencyInjection
{
    /// <summary>
    /// Lifetime = Transient
    /// </summary>
    public interface ITransientDependency { }

    /// <summary>
    /// Lifetime = Scoped
    /// </summary>
    public interface IScopedDependency { }

    /// <summary>
    /// Lifetime = Singleton
    /// </summary>
    public interface ISingletonDependency { }
}