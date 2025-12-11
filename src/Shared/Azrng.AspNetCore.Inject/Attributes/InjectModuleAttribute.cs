namespace Azrng.AspNetCore.Inject.Attributes;

/// <summary>
/// 用于引入依赖模块
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class InjectModuleAttribute : Attribute
{
    public Type ModuleType { get; private init; }

    public InjectModuleAttribute(Type type)
    {
        ModuleType = type;
    }
}

/// <summary>
/// 用于引入依赖模块
/// </summary>
/// <typeparam name="TModule"></typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class InjectModuleAttribute<TModule> : InjectModuleAttribute
    where TModule : IModule
{
    public InjectModuleAttribute() : base(typeof(TModule)) { }
}