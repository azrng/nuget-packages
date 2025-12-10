namespace Azrng.Core.Requests
{
    /// <summary>
    /// 基本请求类dto
    /// </summary>
    /// <typeparam name="T">Data的类型</typeparam>
    /// <typeparam name="TO">Operator里面用户ID的类型</typeparam>
    public class BaseRequestDto<T, TO>
    {
        /// <summary>
        /// 请求数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 登陆人信息
        /// </summary>
        public OperatorDto<TO> UserIdentity { get; set; } = new OperatorDto<TO>();
    }

    /// <summary>
    /// 基本请求类dto
    /// </summary>
    /// <typeparam name="T">Data的类型</typeparam>
    public class BaseRequestDto<T>
    {
        /// <summary>
        /// 请求数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 登陆人信息
        /// </summary>
        public OperatorDto<string> UserIdentity { get; set; } = new OperatorDto<string>();
    }

    /// <summary>
    /// 基本请求类dto
    /// </summary>
    /// <typeparam name="T">Data的类型</typeparam>
    /// <typeparam name="TOperator">登录人信息类</typeparam>
    public class BaseCustomerRequestDto<T, TOperator> where TOperator : class, new()
    {
        /// <summary>
        /// 请求数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 登陆人信息
        /// </summary>
        public TOperator UserIdentity { get; set; } = new TOperator();
    }
}