using Common.YuQueSdk.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.YuQueSdk
{
    public interface IYuQueExtensionHelper
    {
        /// <summary>
        /// 获取知识库下文档目录列表
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
        Task<YuQueResult<List<TopicTree>>> GetRepoTopicTreeMenuAsync(long repositoryId);

        /// <summary>
        /// 保存仓库下文档到某一个路径
        /// </summary>
        /// <param name="repositoryId">仓库标识</param>
        /// <param name="savePath">保存地址</param>
        /// <param name="isSort">是否排序保存，排序保存的话名称会增加三位排序号前缀</param>
        /// <param name="fileContentFunc">文件内容 func</param>
        /// <returns></returns>
        Task<bool> SaveRepositoryDocAsync(long repositoryId, string savePath, bool isSort = false, Func<string, string> fileContentFunc = null);
    }
}