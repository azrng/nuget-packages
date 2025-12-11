//namespace Common.YuQueSdk
//{
//    /// <summary>
//    /// 语雀url常量
//    /// </summary>
//    internal static class YuQueUrlConst
//    {
//        /// <summary>
//        /// 语雀基础的api域名
//        /// </summary>
//        private const string _baseUrl = "https://www.yuque.com/api/v2/";

//        #region 用户

//        /// <summary>
//        /// 获取用户url
//        /// </summary>
//        public const string GetUserUrl = _baseUrl + "users/{0}";

//        #endregion 用户

//        #region 组织

//        /// <summary>
//        /// 获取用户组织
//        /// </summary>
//        public const string GetUserGroupUrl = _baseUrl + "users/{0}/groups";

//        /// <summary>
//        /// 创建组织
//        /// </summary>
//        public const string CreateGroupUrl = _baseUrl + "groups";

//        #endregion 组织

//        #region 知识库

//        /// <summary>
//        /// 获取用户下知识库列表
//        /// </summary>
//        public const string GetUserReposUrl = _baseUrl + "users/{0}/repos";

//        /// <summary>
//        /// 创建用户下知识库列表
//        /// </summary>
//        public const string CreateUserReposUrl = _baseUrl + "users/{0}/repos";

//        /// <summary>
//        /// 获取知识库详情
//        /// </summary>
//        public const string GetReposUrl = _baseUrl + "repos/{0}";

//        /// <summary>
//        /// 更新知识库信息
//        /// </summary>
//        public const string UpdateReposUrl = _baseUrl + "repos/{0}";

//        /// <summary>
//        /// 删除知识库
//        /// </summary>
//        public const string DeleteReposUrl = _baseUrl + "repos/{0}";

//        /// <summary>
//        /// 获取知识库目录
//        /// </summary>
//        public const string GetReposTocUrl = _baseUrl + "repos/{0}/toc";

//        #endregion 知识库

//        #region 文档

//        /// <summary>
//        /// 获取一个仓库的文档列表
//        /// </summary>
//        public const string GetReposDocsListUrl = _baseUrl + "repos/{0}/docs";

//        /// <summary>
//        /// 获取单篇文档url
//        /// </summary>
//        public const string GetReposDocsUrl = _baseUrl + "repos/{0}/docs/{1}?raw=1";

//        /// <summary>
//        /// 创建文档
//        /// </summary>
//        public const string CreateReposDocsUrl = _baseUrl + "repos/{0}/docs";

//        /// <summary>
//        /// 更新文档
//        /// </summary>
//        public const string UpdateReposDocsUrl = _baseUrl + "repos/{0}/docs/{1}";

//        /// <summary>
//        /// 删除文档
//        /// </summary>
//        public const string DeleteReposDocsUrl = _baseUrl + "repos/{0}/docs/{1}";

//        #endregion 文档
//    }
//}