using System;

namespace Azrng.Dapper.Test;

/// <summary>
/// 用户实体
/// </summary>
public class User
{
    /// <summary>
    /// 主键
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string PassWord { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public int Sex { get; set; }

    /// <summary>
    /// 积分
    /// </summary>
    public double Credit { get; set; }

    /// <summary>
    /// 组ID
    /// </summary>
    public long GroupId { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string Creater { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    public string Modifyer { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime ModifyTime { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    public bool Deleted { get; set; }

    /// <summary>
    /// 是否禁用
    /// </summary>
    public bool Disabled { get; set; }
}