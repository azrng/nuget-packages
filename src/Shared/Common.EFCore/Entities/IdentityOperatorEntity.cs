using Azrng.Core.Extension;
using Coldairarrow.Util;
using System;

namespace Azrng.EFCore.Entities
{
    /// <summary>
    /// 标识标识操作者
    /// </summary>
    public abstract class IdentityOperatorEntity : IdentityOperatorEntity<long>
    {
        public IdentityOperatorEntity()
        {
            Id = IdHelper.GetLongId();
        }
    }

    /// <summary>
    /// 标识标识操作者
    /// </summary>
    public abstract class IdentityOperatorEntity<TKey> : IdentityBaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// 创建者账号
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string Updater { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 设置创建人
        /// </summary>
        /// <param name="name">创建人</param>
        /// <param name="dateTime">创建时间</param>
        /// <param name="setUpdater">是否设置更新人</param>
        public void SetCreator(string name, DateTime? dateTime = null, bool setUpdater = true)
        {
            Creator = name;
            CreateTime = dateTime ?? DateTime.Now.ToUnspecifiedDateTime();
            if (setUpdater)
                SetUpdater(name, dateTime);
        }

        /// <summary>
        /// 设置修改人
        /// </summary>
        /// <param name="name">修改人</param>
        /// <param name="dateTime">创建时间</param>
        public void SetUpdater(string name, DateTime? dateTime = null)
        {
            Updater = name;
            UpdateTime = dateTime ?? DateTime.Now.ToUnspecifiedDateTime();
        }

        /// <summary>
        /// 设置创建人
        /// </summary>
        /// <param name="name">创建人</param>
        /// <param name="dateTime">创建时间</param>
        /// <remarks>改为使用SetCreator</remarks>
        [Obsolete]
        public void SetCreater(string name, DateTime? dateTime = null)
        {
            Creator = name;
            CreateTime = dateTime ?? DateTime.Now.ToUnspecifiedDateTime();
            SetModifyer(name, dateTime);
        }

        /// <summary>
        /// 设置修改人
        /// </summary>
        /// <param name="name">修改人</param>
        /// <param name="dateTime">创建时间</param>
        /// <remarks>改为使用SetCreator</remarks>
        [Obsolete]
        public void SetModifyer(string name, DateTime? dateTime = null)
        {
            Updater = name;
            UpdateTime = dateTime ?? DateTime.Now.ToUnspecifiedDateTime();
        }
    }
}