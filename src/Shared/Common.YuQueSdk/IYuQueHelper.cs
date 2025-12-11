using Common.YuQueSdk.Dto;
using Common.YuQueSdk.Dto.Doc;
using Common.YuQueSdk.Dto.Repository;
using Common.YuQueSdk.Dto.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.YuQueSdk
{
    /// <summary>
    /// 语雀帮助类
    /// </summary>
    public interface IYuQueHelper
    {
        #region 用户

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="login">用户名</param>
        /// <returns></returns>
        Task<YuQueResult<GetUserResult>> GetUsersAsync(string login);

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="id">数据的唯一编号</param>
        /// <returns></returns>
        Task<YuQueResult<GetUserResult>> GetUsersAsync(int id);

        #endregion 用户

        #region Group组织

        ///// <summary>
        ///// 获取用户加入的组织
        ///// </summary>
        ///// <param name="login">用户名</param>
        ///// <returns></returns>
        //Task<YuQueResult<string>> GetUserGroup(string login);

        #endregion

        #region 仓库

        /// <summary>
        /// 获取用户下知识库列表
        /// </summary>
        /// <param name="login">用户名</param>
        /// <returns></returns>
        Task<YuQueResult<List<GetUserRepositoryResult>>> GetRepoListByLoginNameAsync(string login);

        /// <summary>
        /// 获取知识库下文档列表
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
        Task<YuQueResult<List<RepositoryDocTopic>>> GetRepoTopicListAsync(long repositoryId);

        #endregion

        #region 文档

        /// <summary>
        /// 根据知识库标识获取知识库下文档列表(不带层级结构)
        /// </summary>
        /// <param name="namespace">知识库标识/知识库地址</param>
        /// <returns></returns>
        Task<YuQueResult<IEnumerable<GetRepositoryDocResult>>> GetRepositoryDocListAsync(string @namespace);

        /// <summary>
        /// 获取单篇文档的详细信息
        /// </summary>
        /// <param name="namespace">仓库ID</param>
        /// <param name="slug">文档Slug</param>
        /// <returns></returns>
        Task<YuQueResult<GetDocsDetailsResponse>> GetReposDocsAsync(long @namespace, string slug);

        #endregion 文档
    }
}