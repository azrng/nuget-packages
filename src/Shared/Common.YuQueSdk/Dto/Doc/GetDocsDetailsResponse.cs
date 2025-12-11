using Newtonsoft.Json;
using System;

namespace Common.YuQueSdk.Dto.Doc
{
    /// <summary>
    /// 获取单篇文档详细信息
    /// </summary>
    public class GetDocsDetailsResponse
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
        [JsonProperty("book_id")]
        public string BookId { get; set; }

        ///// <summary>
        ///// 仓库
        ///// </summary>
        //[JsonProperty("book")]
        //public DockBookInfoDto Book { get; set; }

        /// <summary>
        /// 用户/团队编号
        /// </summary>
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        // 创建人信息
        //public Creator creator { get; set; }

        /// <summary>
        /// 表述了正文的格式[lake,markdown]
        /// </summary>
        [JsonProperty("format")]
        public string Format { get; set; }

        /// <summary>
        /// 正文 Markdown 源代码
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; set; }

        /// <summary>
        /// 草稿 Markdown 源代码
        /// </summary>
        [JsonProperty("body_draft")]
        public string BodyDraft { get; set; }

        /// <summary>
        /// 转换过后的正文 HTML
        /// </summary>
        [JsonProperty("body_html")]
        public string BodyHtml { get; set; }

        /// <summary>
        ///  语雀 lake 格式的文档内容
        /// </summary>
        [JsonProperty("body_lake")]
        public string BodyLake { get; set; }

        [JsonProperty("body_draft_lake")]
        public string BodyDraftLake { get; set; }

        /// <summary>
        /// 公开级别 [0 - 私密, 1 - 公开]
        /// </summary>
        [JsonProperty("public")]
        public int _public { get; set; }

        /// <summary>
        /// 状态 [0 - 草稿, 1 - 发布]
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        /// <summary>
        /// 浏览数
        /// </summary>
        [JsonProperty("view_status")]
        public int Viewstatus { get; set; }

        [JsonProperty("read_status")]
        public int ReadStatus { get; set; }

        /// <summary>
        /// 赞数量
        /// </summary>
        [JsonProperty("likes_count")]
        public int LikesCount { get; set; }

        /// <summary>
        /// 评论数量
        /// </summary>
        [JsonProperty("comments_count")]
        public int CommentsCount { get; set; }

        /// <summary>
        /// 文档内容更新时间
        /// </summary>
        [JsonProperty("content_updated_at")]
        public DateTime ContentUpdatedAt { get; set; }

        ///// <summary>
        ///// 删除时间，未删除为 null
        ///// </summary>
        //[JsonProperty("deleted_at")]
        //public object DeletedAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// PublishedAt(未发布为null)
        /// </summary>
        [JsonProperty("published_at")]
        public DateTime? PublishedAt { get; set; }

        [JsonProperty("first_published_at")]
        public DateTime? FirstPublishedAt { get; set; }

        [JsonProperty("word_count")]
        public int WordCount { get; set; }

        //[JsonProperty("cover")]
        //public object Cover { get; set; }

        /// <summary>
        /// 文档备注
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        //[JsonProperty("custom_description")]
        //public object CustomDescription { get; set; }

        [JsonProperty("hits")]
        public int Hits { get; set; }
    }

    /// <summary>
    /// 仓库信息
    /// </summary>
    public class DockBookInfoDto
    {
        /// <summary>
        /// 仓库标识
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        ///仓库类别
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// slug
        /// </summary>
        [JsonProperty("slug")]
        public string Slug { get; set; }

        /// <summary>
        /// 仓库名
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 用户标识
        /// </summary>
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// 仓库描述
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        //[JsonProperty("book_id")]
        //public int creator_id { get; set; }

        //[JsonProperty("book_id")]
        //public int _public { get; set; }

        //[JsonProperty("book_id")]
        //public int items_count { get; set; }

        //[JsonProperty("book_id")]
        //public int likes_count { get; set; }

        //[JsonProperty("book_id")]
        //public int watches_count { get; set; }

        //[JsonProperty("book_id")]
        //public DateTime content_updated_at { get; set; }

        //[JsonProperty("book_id")]
        //public DateTime updated_at { get; set; }

        //[JsonProperty("book_id")]
        //public DateTime created_at { get; set; }

        //[JsonProperty("book_id")]
        //public string _namespace { get; set; }
    }
}