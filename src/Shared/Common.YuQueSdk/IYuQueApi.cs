using Common.YuQueSdk.Dto;
using Common.YuQueSdk.Dto.Doc;
using Common.YuQueSdk.Dto.Repository;
using Common.YuQueSdk.Dto.User;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.YuQueSdk
{
    /// <summary>
    /// 语雀api api来自：https://www.yuque.com/yuque/developer/api
    /// </summary>
    public interface IYuQueApi
    {
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="login">用户名</param>
        /// <returns></returns>
        [Get("/users/{login}")]
        Task<YuQueResult<GetUserResult>> GetUsersAsync(string login);

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="id">数据的唯一编号</param>
        /// <returns></returns>
        [Get("/users/{id}")]
        Task<YuQueResult<GetUserResult>> GetUsersAsync(int id);

        /// <summary>
        /// 获取用户下知识库列表
        /// </summary>
        /// <param name="login">用户名</param>
        /// <returns></returns>
        [Get("/users/{login}/repos")]
        Task<YuQueResult<List<GetUserRepositoryResult>>> GetRepoListByLoginNameAsync(string login);

        /// <summary>
        /// 获取知识库目录(文档基本信息)
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
        [Get("/repos/{repositoryId}/toc")]
        Task<YuQueResult<List<RepositoryDocTopic>>> GetRepoTopicListAsync(long repositoryId);

        /// <summary>
        /// 获取单篇文档的详细信息
        /// </summary>
        /// <param name="_namespace">仓库ID</param>
        /// <param name="slug">文档Slug</param>
        /// <returns></returns>
        [Get("/repos/{_namespace}/docs/{slug}?raw=1")]
        Task<YuQueResult<GetDocsDetailsResponse>> GetReposDocDetailsAsync(long _namespace, string slug);

        /// <summary>
        /// 获取单篇文档md内容信息
        /// </summary>
        /// <param name="_namespace">仓库ID</param>
        /// <param name="slug">文档Slug</param>
        /// <returns></returns>
        [Get("/repos/{_namespace}/docs/{slug}?raw=1")]
        Task<YuQueResult<GetDocsMdBodyResponse>> GetDocsMdBodyAsync(long _namespace, string slug);

        /// <summary>
        /// 根据知识库标识获取知识库下文档详细列表(带文章简述)
        /// </summary>
        /// <param name="_namespace"></param>
        /// <returns></returns>
        [Get("/repos/{_namespace}/docs")]
        Task<YuQueResult<IEnumerable<GetRepositoryDocResult>>> GetRepositoryDocListAsync(string _namespace);
    }
}