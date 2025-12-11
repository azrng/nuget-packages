namespace Azrng.DistributeLock.Core
{
    /// <summary>
    /// 锁提供程序
    /// </summary>
    public interface ILockProvider
    {
        /// <summary>
        /// 开启分布式锁
        /// </summary>
        /// <param name="lockKey">分布式锁的key</param>
        /// <param name="expire">过期时间，默认5秒</param>
        /// <param name="getLockTimeOut">获取锁等待时间,默认5秒</param>
        /// <param name="autoExtend">是否自动延期</param>
        /// <returns>如果获取不到锁返回null</returns>
        /// <remarks>
        /// 原理：通过使用using的表达式来实现释放锁的效果，拿不到锁不会报错，会返回null值
        /// </remarks>
        Task<IAsyncDisposable?> LockAsync(string lockKey, TimeSpan? expire = null, TimeSpan? getLockTimeOut = null,
            bool autoExtend = true);
    }
}