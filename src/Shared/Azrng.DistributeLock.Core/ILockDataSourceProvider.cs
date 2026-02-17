namespace Azrng.DistributeLock.Core
{
    /// <summary>
    /// 分布式锁数据源提供程序
    /// </summary>
    public interface ILockDataSourceProvider
    {
        /// <summary>
        /// 获取锁
        /// </summary>
        /// <param name="lockKey">锁键</param>
        /// <param name="lockValue">值</param>
        /// <param name="expireTime">过期时间</param>
        /// <param name="getLockTimeOut">获取锁过期时间</param>
        /// <returns></returns>
        Task<bool> TakeLockAsync(string lockKey, string lockValue, TimeSpan expireTime, TimeSpan getLockTimeOut);

        /// <summary>
        /// 延长锁
        /// </summary>
        /// <param name="lockKey">锁键</param>
        /// <param name="lockValue">值</param>
        /// <param name="extendTime">延长时间</param>
        /// <returns>续期是否成功</returns>
        Task<bool> ExtendLockAsync(string lockKey, string lockValue, TimeSpan extendTime);

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="lockKey">锁键</param>
        /// <param name="lockValue">值</param>
        /// <returns></returns>
        Task ReleaseLockAsync(string lockKey, string lockValue);
    }
}