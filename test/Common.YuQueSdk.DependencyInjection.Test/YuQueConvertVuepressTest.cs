// using Azrng.Core;
// using Common.Core;
// using Common.Core.Extension;
// using Common.Extension;
// using Common.Helpers;
// using Common.YuQueSdk.DependencyInjection.Test.Model;
// using Common.YuQueSdk.Dto;
// using Common.YuQueSdk.Enums;
// using System.Text;
// using System.Text.RegularExpressions;
//
// namespace Common.YuQueSdk.DependencyInjection.Test;
//
// /// <summary>
// /// 将语雀转vuepress测试
// /// </summary>
// public class YuQueConvertVuepressTest
// {
//     private readonly IYuQueHelper _yuQueHelper;
//     private readonly IYuQueExtensionHelper _yuQueExtensionHelper;
//     private readonly IHttpClientFactory _httpClientFactory;
//     private readonly IJsonSerializer _jsonSerializer;
//
//     #region 配置
//
//     /// <summary>
//     /// 图片保存地址
//     /// </summary>
//     private const string ImageSavePath = "D:\\temp\\docs\\common";
//
//     /// <summary>
//     /// 分类
//     /// </summary>
//     private const string DefaultCategory = "csharp";
//
//     /// <summary>
//     /// 文件夹编码
//     /// </summary>
//     private const string FolderCode = "csharp";
//
//     /// <summary>
//     /// 是否忽略已经完成的文章
//     /// </summary>
//     private const bool IsIgnoreCompleteDoc = false;
//
//     #endregion
//
//     public YuQueConvertVuepressTest(IYuQueHelper yuQueHelper, IYuQueExtensionHelper yuQueExtensionHelper,
//                                     IHttpClientFactory httpClientFactory, IJsonSerializer jsonSerializer)
//     {
//         _yuQueHelper = yuQueHelper;
//         _yuQueExtensionHelper = yuQueExtensionHelper;
//         _httpClientFactory = httpClientFactory;
//         _jsonSerializer = jsonSerializer;
//     }
//
//     /// <summary>
//     /// 转换整个知识库
//     /// </summary>
//     /// <returns></returns>
//     [Fact]
//     public async Task ConvertTestAsync()
//     {
//         var savePath = "d:\\temp\\docs";
//
//         // 生活
//         var repositoryId = 24424100;
//
//         if (!Directory.Exists(ImageSavePath))
//             Directory.CreateDirectory(ImageSavePath);
//
//         var tree = await _yuQueExtensionHelper.GetRepoTopicTreeMenuAsync(repositoryId);
//         Assert.True(tree.IsSuccess);
//
//         var menuTree = new VuepressMenuDto(DefaultCategory, $"/{FolderCode}/");
//
//         // 写菜单文档
//         await WriteMenuDocAsync(tree.Data, repositoryId, savePath, menuTree);
//
//         var jsMenu = MenuTreeJsHandle(menuTree.children);
//         File.WriteAllText(Path.Combine(savePath, "menu.txt"), jsMenu);
//     }
//
//     /// <summary>
//     /// 转换单个文档
//     /// </summary>
//     /// <returns></returns>
//     [Fact]
//     public async Task ConvertSingleDocAsync()
//     {
//         var savePath = "d:\\temp\\docs";
//
//         // 软件
//         var repositoryId = 20024893;
//         var slug = "uyk4a7";
//
//         if (!Directory.Exists(ImageSavePath))
//             Directory.CreateDirectory(ImageSavePath);
//
//         var resultFileName = await SaveDocsAsync(repositoryId, slug, savePath);
//     }
//
//     /// <summary>
//     /// 树形菜单生成js文件
//     /// </summary>
//     /// <param name="vuepressMenus"></param>
//     /// <returns></returns>
//     private string MenuTreeJsHandle(List<VuepressMenuDto> vuepressMenus)
//     {
//         var jsonStr = _jsonSerializer.ToJson(vuepressMenus);
//
//         jsonStr = jsonStr
//                   .Replace("\"text\"", "text")
//                   .Replace("\"prefix\"", "prefix")
//                   .Replace("\"collapsible\"", "collapsible")
//                   .Replace("\"children\"", "children")
//                   .Replace("{text:\"\",prefix:null,collapsible:true,children:[]}", "");
//
//         const string pattern = @"\{text:\""([^""]+)\""";
//         var matchs = Regex.Matches(jsonStr, pattern);
//         foreach (Match item in matchs)
//         {
//             var currDocsConent = item.Groups[1].Value;
//             jsonStr = jsonStr.Replace($"{{text:\"{currDocsConent}\",prefix:null,collapsible:true,children:[]}}",
//                 $"\"{currDocsConent}\"");
//         }
//
//         var sb = new StringBuilder();
//         foreach (var item in jsonStr)
//         {
//             sb.Append(item);
//             if (item == ',')
//             {
//                 sb.Append(Environment.NewLine);
//             }
//         }
//
//         return sb.ToString();
//     }
//
//     /// <summary>
//     /// 写入菜单文档
//     /// </summary>
//     /// <param name="topicTrees"></param>
//     /// <param name="repositoryId"></param>
//     /// <param name="savePath"></param>
//     /// <returns></returns>
//     private async Task WriteMenuDocAsync(List<TopicTree> topicTrees, long repositoryId, string savePath,
//                                          VuepressMenuDto vuepressMenu)
//     {
//         foreach (var item in topicTrees)
//         {
//             var menuName = FileNameConvertPinYin(item.Title);
//             var currPath = Path.Combine(savePath, menuName);
//             if (item.DocType == DocTypeEnum.Doc)
//             {
//                 // 如果类型是doc 那么它本身保存，子集也保存
//                 var resultFileName = await SaveDocsAsync(repositoryId, item.Slug, savePath);
//
//                 var currMenu = new VuepressMenuDto(resultFileName);
//                 vuepressMenu.AddChild(currMenu);
//             }
//             else if (item.DocType == DocTypeEnum.DocAndMenu)
//             {
//                 var currMenu = new VuepressMenuDto(item.Title, vuepressMenu.prefix + menuName + "/");
//
//                 // 如果类型文档并且是目录，那么就创建文件夹保存现在这个以及子项
//                 var resultFileName = await SaveDocsAsync(repositoryId, item.Slug, currPath);
//
//                 currMenu.AddChild(new VuepressMenuDto(resultFileName));
//                 if (item.Child != null && item.Child.Count > 0)
//                 {
//                     await WriteMenuDocAsync(item.Child, repositoryId, currPath, currMenu);
//                 }
//
//                 vuepressMenu.AddChild(currMenu);
//             }
//             else
//             {
//                 // 菜单的情况直接往下创建文件夹
//                 if (item.Child != null && item.Child.Count > 0)
//                 {
//                     var currMenu = new VuepressMenuDto(item.Title, vuepressMenu.prefix + menuName + "/");
//
//                     await WriteMenuDocAsync(item.Child, repositoryId, currPath, currMenu);
//                     vuepressMenu.AddChild(currMenu);
//                 }
//             }
//         }
//     }
//
//     /// <summary>
//     /// 保存文档到指定目录
//     /// </summary>
//     /// <param name="repositoryId">仓库标识</param>
//     /// <param name="slug">文档标识</param>
//     /// <param name="path">文档保存目录</param>
//     private async Task<string> SaveDocsAsync(long repositoryId, string slug, string path)
//     {
//         if (!Directory.Exists(path))
//             Directory.CreateDirectory(path);
//
//         try
//         {
//             var response = await _yuQueHelper.GetReposDocsAsync(repositoryId, slug);
//             Assert.True(response.IsSuccess);
//
//             var (content, resultFileName) = MdFileAddHeader(response.Data.Id,
//                 response.Data.Title,
//                 response.Data.Body,
//                 response.Data.UpdatedAt,
//                 slug,
//                 "无");
//             if (string.IsNullOrEmpty(resultFileName))
//             {
//                 // 不处理
//                 return string.Empty;
//             }
//
//             var fileName = resultFileName + ".md";
//             var imageHandleContent = await FileImageSaveAsync(content);
//
//             var resultPath = Path.Combine(path, fileName);
//             var fileDirectoryPath = Path.GetDirectoryName(resultPath);
//             if (!Directory.Exists(fileDirectoryPath))
//                 Directory.CreateDirectory(fileDirectoryPath);
//             await File.WriteAllTextAsync(resultPath, imageHandleContent);
//
//             return fileName;
//         }
//         catch (Exception ex)
//         {
//             await Console.Out.WriteLineAsync($"错误信息  {ex.Message}");
//             return "";
//         }
//     }
//
//     /// <summary>
//     /// 给文本内容拼接头部yaml标签
//     /// </summary>
//     /// <returns>（内容，文件名）</returns>
//     private (string content, string fileName) MdFileAddHeader(
//         string fileId,
//         string fileName,
//         string content,
//         DateTime dateTime,
//         string slug,
//         string tag
//     )
//     {
//         // 文章开头标注了over的考虑是否忽略
//         if (content.Length > 6 && content.Substring(0, 6).Contains("over") && IsIgnoreCompleteDoc)
//             return (content, string.Empty);
//
//         // 去除内容中的 <a name="xxx"></a>
//         const string pattern = @"<a name="".*?""></a>";
//         content = Regex.Replace(content, pattern, string.Empty);
//
//         // 文章内容层级应该降低一个层级，原来的二级目录应该降低为三级目录
//         content = content.Replace("# ", "## ");
//
//         // 将里面的br标签给换行一下
//         content = content.Replace("<br />", Environment.NewLine);
//
//         var pinYinFileName = FileNameConvertPinYin(fileName);
//
//         var template =
//             $@"---
// title: {fileName}
// lang: zh-CN
// date: {dateTime.ToString("yyyy-MM-dd")}
// publish: true
// author: azrng
// isOriginal: true
// category:
//   - {DefaultCategory}
// tag:
//   - {tag}
// filename: {pinYinFileName}
// slug: {slug}
// docsId: '{fileId}'
// ---
// ";
//         return (template + content, pinYinFileName);
//     }
//
//     /// <summary>
//     /// 文件内图片提取保存
//     /// </summary>
//     /// <param name="content"></param>
//     /// <returns></returns>
//     private async Task<string> FileImageSaveAsync(string content)
//     {
//         var client = _httpClientFactory.CreateClient();
//         client.DefaultRequestHeaders.Add("Referer", "https://www.yuque.com/");
//         client.DefaultRequestHeaders.Add("User-Agent",
//             "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36");
//
//         var regex = new Regex("!\\[.*?\\]\\((.*?)\\)");
//         var matches = regex.Matches(content);
//         foreach (Match item in matches)
//         {
//             var imageUrl = item.Groups[1].Value;
//             var imageName = string.Empty;
//             if (imageUrl.StartsWith("https://"))
//             {
//                 // 拷贝网络图片
//                 imageName = Path.GetFileName(imageUrl);
//                 var imageNameArray = imageName.Split("#");
//                 imageName = imageNameArray[0];
//
//                 // 图片路径处理，包含问号的截取之前的名字
//                 if (imageName.Contains("?"))
//                     imageName = imageName.Split("?")[0];
//                 var copyNewImagePath = Path.Combine(ImageSavePath, imageName);
//                 if (!File.Exists(copyNewImagePath))
//                 {
//                     var bytes = await client.GetByteArrayAsync(imageUrl);
//                     await File.WriteAllBytesAsync(copyNewImagePath, bytes);
//                 }
//             }
//
//             var imageNewPath = $"/common/{imageName}";
//             content = content.Replace(imageUrl, imageNewPath);
//         }
//
//         return content;
//     }
//
//     /// <summary>
//     /// 文件名处理
//     /// </summary>
//     /// <param name="originFileName"></param>
//     /// <returns></returns>
//     private string FileNameConvertPinYin(string originFileName)
//     {
//         string? pinYinFileName;
//         if (originFileName == "说明")
//             return "readme";
//
//         try
//         {
//             pinYinFileName = PinYinHelper.GetPinyinQuanPin(originFileName)
//                                          .ToLowerInvariant()
//                                          .Replace(".", "_")
//                                          .Replace(" ", "");
//         }
//         catch
//         {
//             pinYinFileName = originFileName.ToLowerInvariant().Replace(".", "_").Replace(" ", "");
//         }
//
//         return pinYinFileName;
//     }
// }