using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.RestSharpClient
{
    public interface IHttpClientHelper
    {
        /// <summary>
        /// get请求
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="header">请求头</param>
        /// <param name="userAgent">用户代理</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string url, Dictionary<string, string> header = null, string userAgent = "");

        /// <summary>
        /// post请求
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">参数(对象/json字符串)</param>
        /// <param name="header">请求头</param>
        /// <param name="userAgent">用户代理</param>
        /// <returns></returns>
        Task<T> PostAsync<T>(string url, object data, Dictionary<string, string> header = null, string userAgent = "");

        /// <summary>
        /// post请求
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">参数(对象/json字符串)</param>
        /// <param name="userAgent">用户代理</param>
        /// <returns></returns>
        Task<T> PostFromDataAsync<T>(string url, Dictionary<string, string> data, string userAgent = "");

        /// <summary>
        /// put请求
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">参数(对象/json字符串)</param>
        /// <param name="header">请求头</param>
        /// <param name="userAgent">用户代理</param>
        /// <returns></returns>
        Task<T> PutAsync<T>(string url, object data, Dictionary<string, string> header = null, string userAgent = "");

        /// <summary>
        /// delete请求
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="header">请求头</param>
        /// <param name="userAgent">用户代理</param>
        /// <returns></returns>
        Task<T> DeleteAsync<T>(string url, Dictionary<string, string> header = null, string userAgent = "");


        /// <summary>
        /// 下载请求
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="header">请求头</param>
        /// <param name="userAgent">用户代理</param>
        byte[] DownloadData(string url, Dictionary<string, string> header = null, string userAgent = "");
    }
}