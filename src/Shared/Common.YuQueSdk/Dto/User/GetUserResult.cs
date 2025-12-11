using Newtonsoft.Json;
using System;

namespace Common.YuQueSdk.Dto.User
{
    /// <summary>
    /// 获取用户返回类
    /// </summary>
    public class GetUserResult
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// 帐号ID
        /// </summary>
        [JsonProperty("account_id")]
        public int account_id { get; set; }

        /// <summary>
        /// 登录名
        /// </summary>
        [JsonProperty("login")]
        public string Login { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 头像地址
        /// </summary>
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        /// <summary>
        ///知识库数量
        /// </summary>
        [JsonProperty("books_count")]
        public int BooksCount { get; set; }

        /// <summary>
        /// 公开的知识库数量
        /// </summary>
        [JsonProperty("public_books_count")]
        public int PublicBooksCount { get; set; }

        //[JsonProperty("id")]
        //public int followers_count { get; set; }

        //[JsonProperty("following_count")]
        //public int following_count { get; set; }

        /// <summary>
        /// 是否公开
        /// </summary>
        [JsonProperty("public")]
        public int _Public { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [JsonProperty("updated_at")]
        public DateTime UpdatedTime { get; set; }
    }

}