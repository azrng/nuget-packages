// using Common.Helpers;
// using System.Text.RegularExpressions;
//
// namespace Common.YuQueSdk.DependencyInjection.Test;
//
// public class YuQueExtensionHelperTest
// {
//     private readonly IYuQueExtensionHelper _extensionHelper;
//     private readonly IYuQueHelper _yuQueHelper;
//
//     public YuQueExtensionHelperTest(IYuQueExtensionHelper helper, IYuQueHelper yuQueHelper)
//     {
//         _extensionHelper = helper;
//         _yuQueHelper = yuQueHelper;
//     }
//
//     /// <summary>
//     /// 获取用户指定仓库内文档目录
//     /// </summary>
//     [Fact]
//     public async Task GetMenuList_ReturnOk()
//     {
//         var repositoryId = 24840528;
//         var response = await _extensionHelper.GetRepoTopicTreeMenuAsync(repositoryId);
//         Assert.True(response.Data.Count > 0);
//     }
//
//     /// <summary>
//     /// 保存指定仓库内容到指定目录
//     /// </summary>
//     [Fact]
//     public async Task SaveRepositoryDoc_ReturnOk()
//     {
//         var repositoryId = 20024893;
//         var childFolder = "软件集";
//         var response = await _extensionHelper.SaveRepositoryDocAsync(repositoryId, $"d://temp//docs//{childFolder}", false);
//         Assert.True(response);
//     }
//
//     /// <summary>
//     /// 保存指定用户下所有仓库内容到指定目录
//     /// </summary>
//     [Fact]
//     public async Task SaveAllRepositoryDoc_ReturnOk()
//     {
//         var allRep = await _yuQueHelper.GetRepoListByLoginNameAsync("azrng");
//         foreach (var item in allRep.Data)
//         {
//             var response = await _extensionHelper.SaveRepositoryDocAsync(item.Id, $"d://temp//docs//{item.Name}", true);
//             await Task.Delay(2000);
//         }
//         Assert.True(allRep.Data.Any());
//     }
//
//     /// <summary>
//     /// 递归文件处理(将指定内容替换为带头部yaml标签的格式)并修改文件名
//     /// </summary>
//     /// <returns></returns>
//     [Fact]
//     public void MdHeaderHandler()
//     {
//         var rootPath = "D:\\temp\\docs\\11111";
//         DirectoriesHandler(rootPath, rootPath, string.Empty, true);
//     }
//
//     #region 私有函数
//
//     /// <summary>
//     /// 目录处理
//     /// </summary>
//     /// <param name="rootPath"></param>
//     /// <param name="category"></param>
//     /// <param name="isFirst"></param>
//     private void DirectoriesHandler(string rootPath, string originRootPath, string category, bool isFirst)
//     {
//         foreach (var path in Directory.GetDirectories(rootPath))
//         {
//             if (path.EndsWith("vuepress"))
//                 continue;
//
//             if (isFirst)
//             {
//                 var fileName = Path.GetFileName(path); // 获取文件名
//                 DirectoriesHandler(path, originRootPath, fileName, false);
//             }
//             else
//             {
//                 DirectoriesHandler(path, originRootPath, category, false);
//             }
//         }
//
//         FileHandler(rootPath, originRootPath, category);
//     }
//
//     /// <summary>
//     /// 文件处理保存
//     /// </summary>
//     /// <param name="rootPath"></param>
//     /// <param name="category"></param>
//     private void FileHandler(string rootPath, string originRootPath, string category)
//     {
//         var i = 1;
//         var currTime = DateTime.Now.ToString("yyyy-MM-dd");
//         foreach (var path in Directory.GetFiles(rootPath))
//         {
//             var fileName = Path.GetFileName(path); // 获取文件名
//             var fileContent = File.ReadAllText(path); // 获取文件内容
//             var (content, resultFileName) = MdFileAddHeader(i, fileName, fileContent, currTime, category, "无");
//             i++;
//             var resultPath = path.Replace(originRootPath, Path.Combine(originRootPath, "new"))
//                 .Replace(fileName, resultFileName + ".md");
//
//             var fileDirectoryPath = Path.GetDirectoryName(resultPath);
//             if (!Directory.Exists(fileDirectoryPath))
//                 Directory.CreateDirectory(fileDirectoryPath);
//
//             File.WriteAllText(resultPath, content);
//         }
//     }
//
//     /// <summary>
//     /// 给文本内容拼接头部yaml标签
//     /// </summary>
//     /// <returns></returns>
//     private (string content, string fileName) MdFileAddHeader(int order, string fileName, string content, string date, string category, string tag)
//     {
//         if (content.StartsWith("---"))
//             return (content, string.Empty);
//
//         // 文件名命令格式：000+文件名
//
//         // 标题根据文件名，然后去除后缀
//         var title = fileName.Replace(".md", "");
//
//         // 去除内容中的 <a name="xxx"></a>
//         const string pattern = @"<a name="".*?""></a>";
//         content = Regex.Replace(content, pattern, string.Empty);
//
//         // 文章内容层级应该降低一个层级，原来的二级目录应该降低为三级目录
//
//         var pinYinFileName = string.Empty;
//         try
//         {
//             pinYinFileName = PinYinHelper.GetPinyinQuanPin(title).ToLowerInvariant().Replace(".", "_");
//         }
//         catch
//         {
//             pinYinFileName = title.ToLowerInvariant().Replace(".", "_");
//         }
//
//         var template = $@"---
// title: {title}
// lang: zh-CN
// date: {date}
// publish: true
// author: azrng
// order: {order.ToString().PadLeft(3, '0')}
// category:
//   - {category}
// tag:
//   - {tag}
// filename: {pinYinFileName}
// ---
// ";
//         return (template + content, pinYinFileName);
//     }
//
//     #endregion
// }