using Coldairarrow.Util;
using System;

namespace Azrng.EFCore.Entities
{
    /// <summary>
    /// 标识操作者 状态
    /// </summary>
    public abstract class IdentityOperatorStatusEntity : IdentityOperatorStatusEntity<long>
    {
        public IdentityOperatorStatusEntity()
        {
            Id = IdHelper.GetLongId();
        }
    }

    /// <summary>
    /// 标识操作者 状态
    /// </summary>
    public abstract class IdentityOperatorStatusEntity<TKey> : IdentityOperatorEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// 设置删除状态
        /// </summary>
        /// <param name="name">操作人</param>
        /// <param name="dateTime">删除时间</param>
        public void SetDeleted(string name, DateTime? dateTime = null)
        {
            Deleted = true;
            SetUpdater(name, dateTime);
        }

        /// <summary>
        /// 设置启禁用状态
        /// </summary>
        /// <param name="name">操作人</param>
        /// <param name="disabled">状态</param>
        /// <param name="dateTime">启用/禁用时间</param>
        public void SetDisabled(bool disabled, string name, DateTime? dateTime = null)
        {
            Disabled = disabled;
            SetUpdater(name, dateTime);
        }
    }
}