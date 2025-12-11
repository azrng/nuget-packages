using Common.YuQueSdk.Dto;
using Common.YuQueSdk.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Common.YuQueSdk
{
    public class YuQueExtensionHelper : IYuQueExtensionHelper
    {
        private readonly ILogger<YuQueExtensionHelper> _logger;
        private readonly IYuQueApi _yuQueApi;

        public YuQueExtensionHelper(ILogger<YuQueExtensionHelper> logger, IYuQueApi yuQueApi)
        {
            _logger = logger;
            _yuQueApi = yuQueApi;
        }

        public async Task<YuQueResult<List<TopicTree>>> GetRepoTopicTreeMenuAsync(long repositoryId)
        {
            var response = await _yuQueApi.GetRepoTopicListAsync(repositoryId);
            if (!response.IsSuccess)
                return new YuQueResult<List<TopicTree>> { Message = response.Message };

            var firstList = response.Data.Where(x => x.Level == 0); //第一级
            _logger.LogInformation($"获取树形菜单目录 存在一级目录{firstList.Count()}个");
            return new YuQueResult<List<TopicTree>>
            {
                Data = ProcessTree(firstList, response.Data),
                Message = "成功"
            };
        }

        public async Task<bool> SaveRepositoryDocAsync(long repositoryId, string savePath, bool isSort = false,
            Func<string, string> fileContentFunc = null)
        {
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            // 预计目标  将对应的文档保存到本地 文档内的图片也保存到本地，并且替换带文档中

            // 获取知识库下文档的树形结构
            var tree = await GetRepoTopicTreeMenuAsync(repositoryId);
            if (!tree.IsSuccess)
            {
                _logger.LogError($"获取属性目录出错 msg:{tree.Message}");
            }
            try
            {
                await WriteMenuDocAsync(tree.Data, repositoryId, savePath, isSort, fileContentFunc);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存文档信息出错  msg:{ex.Message} starce:{ex.StackTrace}");
                return false;
            }
        }

        #region 私有函数

        /// <summary>
        /// 递归处理树结构
        /// </summary>
        /// <param name="childs"></param>
        /// <param name="allList"></param>
        /// <returns></returns>
        private List<TopicTree> ProcessTree(IEnumerable<RepositoryDocTopic> childs,
            IReadOnlyCollection<RepositoryDocTopic> allList)
        {
            var result = new List<TopicTree>();

            foreach (var item in childs)
            {
                var topicTree = new TopicTree(item.Type, item.Title, item.Uuid, item.Parent_uuid, item.Level, item.Slug);
                var childList = allList.Where(x => x.Parent_uuid == item.Uuid);
                if (childList.Any())
                {
                    topicTree.SetChild(ProcessTree(childList, allList));
                }

                result.Add(topicTree);
            }

            return result;
        }

        /// <summary>
        /// 写入菜单文档
        /// </summary>
        /// <param name="topicTrees"></param>
        /// <param name="repositoryId"></param>
        /// <param name="savePath"></param>
        /// <param name="isSort"></param>
        /// <param name="fileContentFunc"></param>
        /// <returns></returns>
        private async Task WriteMenuDocAsync(List<TopicTree> topicTrees, long repositoryId, string savePath, bool isSort,
            Func<string, string> fileContentFunc = null)
        {
            var order = 1;
            foreach (var item in topicTrees)
            {
                var menu = isSort ? order.ToString().PadLeft(3, '0') + "0_" + item.Title : item.Title;
                var currPath = Path.Combine(savePath, menu);
                if (item.DocType == DocTypeEnum.Doc)
                {
                    // 如果类型是doc 那么它本身保存，子集也保存
                    await SaveDocsAsync(repositoryId, item.Uuid, item.Slug, savePath, isSort, order, fileContentFunc);
                }
                else if (item.DocType == DocTypeEnum.DocAndMenu)
                {
                    // 如果类型文档并且是目录，那么就创建文件夹保存现在这个以及子项
                    await SaveDocsAsync(repositoryId, item.Uuid, item.Slug, currPath, isSort, order, fileContentFunc);
                    if (item.Child.Count > 0)
                    {
                        await WriteMenuDocAsync(item.Child, repositoryId, currPath, isSort, fileContentFunc);
                    }
                }
                else
                {
                    // 菜单的情况直接往下创建文件夹
                    if (item.Child.Count > 0)
                    {
                        await WriteMenuDocAsync(item.Child, repositoryId, currPath, isSort, fileContentFunc);
                    }
                }
                order++;
            }
        }

        /// <summary>
        /// 保存文档到指定目录
        /// </summary>
        /// <param name="repositoryId">仓库标识</param>
        /// <param name="uuId">文档标识</param>
        /// <param name="slug">文档标识</param>
        /// <param name="path">文档保存目录</param>
        /// <param name="isSort">是否排序</param>
        /// <param name="order">排序号</param>
        /// <param name="fileContentFunc"></param>
        private async Task SaveDocsAsync(long repositoryId, string uuId, string slug, string path, bool isSort, int order,
            Func<string, string> fileContentFunc = null)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            try
            {
                var response = await _yuQueApi.GetDocsMdBodyAsync(repositoryId, slug);
                if (response.IsSuccess)
                {
                    var fileName = isSort ? order.ToString().PadLeft(3, '0') + "0_" + response.Data.Title + ".md" :
                       response.Data.Title + ".md";
                    await File.WriteAllTextAsync(Path.Combine(path, fileName),
                        fileContentFunc != null ? fileContentFunc.Invoke(response.Data.Body) : response.Data.Body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存文档信息出错 msg:{ex.Message} 当前文档标识：{uuId} slug:{slug}");
            }
        }

        #endregion
    }
}