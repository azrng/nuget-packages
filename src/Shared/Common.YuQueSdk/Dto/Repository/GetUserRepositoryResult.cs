using Newtonsoft.Json;
using System;

namespace Common.YuQueSdk.Dto.Repository
{
    /// <summary>
    /// 获取用户下的知识库列表
    /// </summary>
    public class GetUserRepositoryResult
    {
        /// <summary>
        /// 仓库编号
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// 类型 [Book - 文档]
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// 仓库路径
        /// </summary>
        [JsonProperty("slug")]
        public string Slug { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 介绍
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// 公开状态 [1 - 公开, 0 - 私密]
        /// </summary>
        [JsonProperty("public")]
        public int @Public { get; set; }

        /// <summary>
        /// 喜欢数量
        /// </summary>
        [JsonProperty("likes_count")]
        public int LikesCount { get; set; }

        /// <summary>
        /// 订阅数量
        /// </summary>
        [JsonProperty("watches_count")]
        public int WatchesCount { get; set; }

        /// <summary>
        /// 内容最后更新时间
        /// </summary>
        [JsonProperty("content_updated_at")]
        public DateTime ContentUpdatedAt { get; set; }

        /// <summary>
        /// 仓库完整路径
        /// </summary>
        [JsonProperty("namespace")]
        public string Namespace { get; set; }
    }
}