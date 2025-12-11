using Newtonsoft.Json;
using System;

namespace Common.YuQueSdk.Dto.Doc
{
    /// <summary>
    /// 获取仓库下文档列表返回类
    /// </summary>
    public class GetRepositoryDocResult
    {
        /// <summary>
        /// 文档编号
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// 文档路径
        /// </summary>
        [JsonProperty("slug")]
        public string Slug { get; set; }

        /// <summary>
        ///  标题
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 简述
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// 知识库ID
        /// </summary>
        [JsonProperty("book_id")]
        public int BookId { get; set; }

        /// <summary>
        /// 描述了正文的格式 [asl, markdown]
        /// </summary>
        [JsonProperty("format")]
        public string Format { get; set; }

        /// <summary>
        ///  是否公开 [1 - 公开, 0 - 私密]
        /// </summary>
        [JsonProperty("public")]
        public int @public { get; set; }

        /// <summary>
        /// 状态 [1 - 正常, 0 - 草稿]
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        ///// <summary>
        /////
        ///// </summary>
        //public int view_status { get; set; }

        /// <summary>
        ///阅读状态
        /// </summary>
        [JsonProperty("read_status")]
        public int ReadStatus { get; set; }

        /// <summary>
        /// 喜欢数量
        /// </summary>
        [JsonProperty("likes_count")]
        public int LikesCount { get; set; }

        /// <summary>
        /// 阅读数
        /// </summary>
        [JsonProperty("read_count")]
        public int ReadCount { get; set; }

        /// <summary>
        /// 评论数量
        /// </summary>
        [JsonProperty("comments_count")]
        public int CommentsCount { get; set; }

        /// <summary>
        /// 内容最后更新时间
        /// </summary>
        [JsonProperty("content_updated_at")]
        public DateTime ContentUpdatedAt { get; set; }

        ///// <summary>
        /////
        ///// </summary>
        //public string published_at { get; set; }
        ///// <summary>
        /////
        ///// </summary>
        //public string first_published_at { get; set; }
        ///// <summary>
        /////
        ///// </summary>
        //public int draft_version { get; set; }

        ///// <summary>
        /////
        ///// </summary>
        //public int last_editor_id { get; set; }
        /// <summary>
        /// 字数
        /// </summary>
        [JsonProperty("word_count")]
        public int WordCount { get; set; }

        ///// <summary>
        /////
        ///// </summary>
        //public string cover { get; set; }

        /// <summary>
        ///自定义描述
        /// </summary>
        [JsonProperty("custom_description")]
        public string CustomDescription { get; set; }
    }
}