using System;
using System.Collections.Generic;
using System.Text;

namespace Common.YuQueSdk.Dto
{
    /// <summary>
    /// 文章详情的数据
    /// </summary>
    public class DocDetailData
    {
        /// <summary>
        /// 文档编号
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 文档路径
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 仓库编号就是repoid
        /// </summary>
        public string Book_Id { get; set; }

        /// <summary>
        /// 仓库信息
        /// </summary>
        public object Book { get; set; }

        /// <summary>
        /// 用户/团队编号
        /// </summary>
        public string User_Id { get; set; }

        /// <summary>
        /// 用户/团队信息
        /// </summary>
        public object User { get; set; }

        /// <summary>
        /// 表述了正文的格式[lake,markdown]
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// 正文 Markdown 源代码
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 草稿 Markdown 源代码
        /// </summary>
        public string Body_Draft { get; set; }

        /// <summary>
        /// 转换过后的正文 HTML 
        /// </summary>
        public string Body_Html { get; set; }

        /// <summary>
        /// 语雀 lake 格式的文档内容
        /// </summary>
        public string Body_Lake { get; set; }

        /// <summary>
        /// 文档创建人 User Id
        /// </summary>
        public string Creator_Id { get; set; }

        /// <summary>
        /// 公开级别 [0 - 私密, 1 - 公开]
        /// </summary>
        public int Public { get; set; }

        /// <summary>
        /// 状态 [0 - 草稿, 1 - 发布]
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 赞数量
        /// </summary>
        public int Likes_Count { get; set; }

        /// <summary>
        /// 评论数量
        /// </summary>
        public int Comments_Count { get; set; }

        /// <summary>
        /// 文档内容更新时间
        /// </summary>
        public DateTime Content_Updated_At { get; set; }

        /// <summary>
        /// 删除时间，未删除为 null 
        /// </summary>
        public DateTime? Deleted_At { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Created_At { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime Updated_At { get; set; }
    }
}
