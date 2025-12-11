// using System.Net.Http;
// using System.Threading;
// using System.Threading.Tasks;
//
// namespace Common.HttpClients
// {
//     /// <summary>
//     /// 自定义请求拦截(做一个拦截器对请求和响应做出自定义全局配置)
//     /// </summary>
//     public class CustomerDelegatingHandler : DelegatingHandler
//     {
//         protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//         {
//             try
//             {
//                 // request 自定义全局请求url  比如这点url是动态从注册发现中获取的，那么这点就可以动态去拼接url地址
//                 return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
//             }
//             catch
//             {
//                 throw;
//             }
//         }
//     }
// }