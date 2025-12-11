using Coldairarrow.Util;
using System;

namespace Azrng.EFCore.Entities
{
    /// <summary>
    /// 基类标识
    /// </summary>
    public abstract class IdentityBaseEntity : IdentityBaseEntity<long>
    {
        protected IdentityBaseEntity()
        {
            Id = IdHelper.GetLongId();
        }
    }

    /// <summary>
    /// 基类标识
    /// </summary>
    public abstract class IdentityBaseEntity<TKey> : IEntity where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// ID主键
        /// </summary>
        public virtual TKey Id { get; set; }
    }
}