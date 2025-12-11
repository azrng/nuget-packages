using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Common.RestSharpClient
{
    public class HttpClientHelper : IHttpClientHelper
    {
        ///<inheritdoc cref="IHttpClientHelper.GetAsync{T}(string, Dictionary{string, string}, string)"/>
        public async Task<T> GetAsync<T>(string url, Dictionary<string, string> header = null, string userAgent = "")
        {
            return await SendAsync<T>(Method.GET, url, userAgent, request =>
             {
                 if (header is null)
                     request.AddHeader("Content-Type", "application/json");
                 else
                     request.AddHeaders(header);
             });
        }

        ///<inheritdoc cref="IHttpClientHelper.PostAsync{T}(string, object, Dictionary{string, string}, string)"/>
        public async Task<T> PostAsync<T>(string url, object data, Dictionary<string, string> header = null, string userAgent = "")
        {
            var vm = data is string ? data : JsonConvert.SerializeObject(data);
            return await SendAsync<T>(Method.POST, url, userAgent, request =>
             {
                 if (header is null)
                     request.AddHeader("Content-Type", "application/json");
                 else
                     request.AddHeaders(header);

                 request.AddParameter("application/json", vm, ParameterType.RequestBody);
             });
        }

        ///<inheritdoc cref="IHttpClientHelper.PostFromDataAsync{T}(string, Dictionary{string, string}, string)"/>
        public async Task<T> PostFromDataAsync<T>(string url, Dictionary<string, string> data, string userAgent = "")
        {
            return await SendAsync<T>(Method.POST, url, userAgent, request =>
             {
                 request.AlwaysMultipartFormData = true;
                 foreach (var item in data)
                 {
                     request.AddParameter(item.Key, item.Value);
                 }
             });
        }

        ///<inheritdoc cref="IHttpClientHelper.PutAsync{T}(string, object, Dictionary{string, string}, string)"/>
        public async Task<T> PutAsync<T>(string url, object data, Dictionary<string, string> header = null, string userAgent = "")
        {
            var vm = data is string ? data : JsonConvert.SerializeObject(data);
            return await SendAsync<T>(Method.PUT, url, userAgent, request =>
             {
                 if (header is null)
                     request.AddHeader("Content-Type", "application/json");
                 else
                     request.AddHeaders(header);

                 request.AddParameter("application/json", vm, ParameterType.RequestBody);
             });
        }

        ///<inheritdoc cref="IHttpClientHelper.DeleteAsync{T}(string, Dictionary{string, string}, string)"/>
        public async Task<T> DeleteAsync<T>(string url, Dictionary<string, string> header = null, string userAgent = "")
        {
            return await SendAsync<T>(Method.DELETE, url, userAgent, request =>
             {
                 if (header is null)
                     request.AddHeader("Content-Type", "application/json");
                 else
                     request.AddHeaders(header);
             });
        }

        ///<inheritdoc cref="IHttpClientHelper.DownloadData(string, Dictionary{string, string}, string)"/>
        public byte[] DownloadData(string url, Dictionary<string, string> header = null, string userAgent = "")
        {
            var client = new RestClient(url)
            {
                Timeout = -1
            };
            var request = new RestRequest(Method.GET);
            if (header is null)
                request.AddHeader("Content-Type", "application/json");
            else
                request.AddHeaders(header);
            if (!string.IsNullOrWhiteSpace(userAgent))
                client.UserAgent = userAgent;

            return client.DownloadData(request);
        }

        #region 私有方法

        /// <summary>
        /// 公共请求私有方法
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="type">请求类型</param>
        /// <param name="url">请求地址</param>
        /// <param name="action">委托</param>
        /// <returns></returns>
        private async Task<T> SendAsync<T>(Method type, string url, string userAgent, Action<RestRequest> action)
        {
            var client = new RestClient(url)
            {
                Timeout = -1
            };
            var request = new RestRequest(type);
            action.Invoke(request);
            if (!string.IsNullOrWhiteSpace(userAgent))
                client.UserAgent = userAgent;

            IRestResponse response = await client.ExecuteAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return (T)Convert.ChangeType(response.ErrorMessage, typeof(T));
            }
            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(response.Content, typeof(T));
            }
            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        #endregion
    }
}