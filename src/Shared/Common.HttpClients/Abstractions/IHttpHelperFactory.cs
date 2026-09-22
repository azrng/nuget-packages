namespace Common.HttpClients
{
    /// <summary>
    /// HTTP 客户端工厂，支持按名称创建不同的 IHttpHelper 实例
    /// </summary>
    public interface IHttpHelperFactory
    {
        /// <summary>
        /// 按名称创建 IHttpHelper 实例
        /// </summary>
        /// <param name="name">客户端名称（注册时指定）</param>
        /// <returns>IHttpHelper 实例</returns>
        IHttpHelper CreateClient(string name);
    }
}
