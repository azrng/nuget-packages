namespace CommonCollect.Net
{
    public class HttpHelper
    {
        /*
         HttpRequest、WebClient和HttpClient的关系：HttpRequest是基层的请求方式，WebClient是对HttpRequest的简化封装，
         在WebClient中有对HttpRequest的默认设置；HttpClient是重写的请求方式，相对于HttpRequest更简单实现异步请求，是.NetCore中更推崇的方式。
             */


        //#region GET
        ///// <summary>
        ///// GET
        ///// </summary>
        ///// <param name="serviceAddress">地址</param>
        ///// <returns></returns>
        //public static string Get(string serviceAddress)
        //{
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceAddress);
        //    request.Method = "GET";
        //    request.ContentType = "text/html;charset=UTF-8";//设置请求头
        //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        //    Stream myResponseStream = response.GetResponseStream();
        //    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
        //    string retString = myStreamReader.ReadToEnd();
        //    myStreamReader.Close();
        //    myResponseStream.Close();
        //    return retString;
        //}
        //#endregion

        //#region WebRequest的post请求 zyp
        //public static string WebRequestPost(string url, string jsonparam)
        //{
        //    //定义request并设置request的路径
        //    WebRequest request = WebRequest.Create(url);
        //    request.Method = "post";

        //    //设置参数的编码格式，解决中文乱码
        //    byte[] byteArray = Encoding.UTF8.GetBytes(jsonparam);

        //    //设置request的MIME类型及内容长度
        //    request.ContentType = "application/json";
        //    request.ContentLength = byteArray.Length;

        //    //打开request字符流
        //    Stream dataStream = request.GetRequestStream();
        //    dataStream.Write(byteArray, 0, byteArray.Length);
        //    dataStream.Close();

        //    //定义response为前面的request响应
        //    WebResponse response = request.GetResponse();

        //    //获取相应的状态代码
        //    Console.WriteLine(((HttpWebResponse)response).StatusDescription);

        //    //定义response字符流
        //    dataStream = response.GetResponseStream();
        //    StreamReader reader = new StreamReader(dataStream);
        //    return reader.ReadToEnd();//读取所有
        //}
        //#endregion

        //#region POST请求
        ///// <summary>
        ///// POST请求
        ///// </summary>
        ///// <param name="serviceAddress">地址</param>
        ///// <param name="strContent">字符串</param>
        ///// <returns></returns>
        //public static string Post(string serviceAddress, string strContent)
        //{
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceAddress);

        //    request.Method = "POST";
        //    request.ContentType = "application/json";
        //    using (StreamWriter dataStream = new StreamWriter(request.GetRequestStream()))
        //    {
        //        dataStream.Write(strContent);
        //        dataStream.Close();
        //    }
        //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        //    string encoding = response.ContentEncoding;
        //    if (encoding == null || encoding.Length < 1)
        //    {
        //        encoding = "UTF-8"; //默认编码  
        //    }
        //    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
        //    string retString = reader.ReadToEnd();

        //    return retString;
        //}
        //#endregion

        //#region POST请求支持重定向
        /////// <summary>
        /////// POST请求支持重定向
        /////// </summary>
        /////// <param name="posturl">地址</param>
        /////// <param name="postData">参数</param>
        /////// <param name="contentType">类型1是[application/json]、2是 [application/x-www-form-urlencoded]</param>
        /////// <returns></returns>
        ////public static string Post(string posturl, string postData, int contentType = 2)
        ////{
        ////    Stream outstream = null;
        ////    Stream instream = null;
        ////    StreamReader sr = null;
        ////    HttpWebResponse response = null;
        ////    HttpWebRequest request = null;
        ////    Encoding encoding = Encoding.GetEncoding("utf-8");
        ////    byte[] data = encoding.GetBytes(postData);
        ////    try
        ////    {
        ////        // 设置参数
        ////        request = WebRequest.Create(posturl) as HttpWebRequest;
        ////        CookieContainer cookieContainer = new CookieContainer();

        ////        request.CookieContainer = cookieContainer;
        ////        request.AllowAutoRedirect = false;
        ////        request.ProtocolVersion = HttpVersion.Version11;
        ////        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
        ////        //request.Host = "";
        ////        //request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:68.0) Gecko/20100101 Firefox/68.0";
        ////        //request.Credentials = CredentialCache.DefaultCredentials;
        ////        request.Method = "POST";
        ////        request.ContentType = contentType==1? "[application/json]":contentType==2? "[application/x-www-form-urlencoded]" : "";
        ////        request.ContentLength = data.Length;
        ////        outstream = request.GetRequestStream();
        ////        outstream.Write(data, 0, data.Length);
        ////        outstream.Close();
        ////        //发送请求并获取相应回应数据
        ////        response = request.GetResponse() as HttpWebResponse;
        ////        //直到request.GetResponse()程序才开始向目标网页发送Post请求
        ////        instream = response.GetResponseStream();
        ////        sr = new StreamReader(instream, encoding);
        ////        //返回结果网页（html）代码
        ////        string content = sr.ReadToEnd();
        ////        string err = string.Empty;
        ////        return "";
        ////    }
        ////    catch (WebException ex)
        ////    {
        ////        // 302重定向
        ////        return null; //ex.Response.Headers["Location"].ToString();
        ////    }
        ////}
        //#endregion

        //#region HttpClientGet请求
        ///// <summary>
        ///// HttpClientGet请求  
        ///// </summary>
        ///// <param name="address"></param>
        ///// <returns></returns>
        //public static async Task<string> GetByHttpClientAsync(string url)
        //{
        //    HttpClientHandler handler = new HttpClientHandler
        //    {
        //        //设置是否发送凭证信息，有的服务器需要验证身份，不是所有服务器需要
        //        UseDefaultCredentials = false
        //    };
        //    using (HttpClient httpClient = new HttpClient(handler))
        //    {
        //        HttpRequestMessage message = new HttpRequestMessage();
        //        message.Method = HttpMethod.Get;
        //        message.RequestUri = new Uri(url);
        //        var result = httpClient.SendAsync(message).Result;
        //        //result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        //        return await result.Content.ReadAsStringAsync();
        //    }
        //}
        //#endregion

        //#region HttpClientPost请求
        ///// <summary>
        ///// HttpClientPost请求
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <param name="json">请求内容</param>
        ///// <returns></returns>
        //public static async Task<string> PostByHttpClientAsync(string address, string json)
        //{
        //    Task<string> result = null;
        //    HttpClientHandler handler = new HttpClientHandler
        //    {
        //        //设置是否发送凭证信息，有的服务器需要验证身份，不是所有服务器需要
        //        UseDefaultCredentials = false
        //    };
        //    HttpClient httpClient = new HttpClient(handler);
        //    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        //    HttpResponseMessage response = await httpClient.PostAsync(address, httpContent);
        //    //response.EnsureSuccessStatusCode();
        //    if (response.StatusCode == HttpStatusCode.OK)
        //    {
        //        //回复直接读取成字符串
        //        return await response.Content.ReadAsStringAsync();
        //    }
        //    else
        //    {
        //        return result.Result;
        //    }

        //}
        //#endregion

        //#region Put
        ///// <summary>
        ///// HTTP Put方式请求数据.
        ///// </summary>
        ///// <param name="url">URL.</param>
        ///// <returns></returns>
        //public static string HttpPut(string url, string param = null)
        //{
        //    HttpWebRequest request;

        //    //如果是发送HTTPS请求  
        //    if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
        //    {
        //        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
        //        request = WebRequest.Create(url) as HttpWebRequest;
        //        request.ProtocolVersion = HttpVersion.Version10;
        //    }
        //    else
        //    {
        //        request = WebRequest.Create(url) as HttpWebRequest;
        //    }
        //    request.Method = "PUT";
        //    request.ContentType = "application/x-www-form-urlencoded";
        //    request.Accept = "*/*";
        //    request.Timeout = 15000;
        //    request.AllowAutoRedirect = false;

        //    StreamWriter requestStream = null;
        //    WebResponse response = null;
        //    string responseStr = null;

        //    try
        //    {
        //        requestStream = new StreamWriter(request.GetRequestStream());
        //        requestStream.Write(param);
        //        requestStream.Close();

        //        response = request.GetResponse();
        //        if (response != null)
        //        {
        //            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
        //            responseStr = reader.ReadToEnd();
        //            reader.Close();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        request = null;
        //        requestStream = null;
        //        response = null;
        //    }

        //    return responseStr;
        //}
        //#endregion

        //#region Delete
        ///// <summary>
        ///// HTTP Delete方式请求数据.
        ///// </summary>
        ///// <param name="url">URL.</param>
        ///// <returns></returns>
        //public static string HttpDelete(string url, string param = null)
        //{
        //    HttpWebRequest request;

        //    //如果是发送HTTPS请求  
        //    if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
        //    {
        //        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
        //        request = WebRequest.Create(url) as HttpWebRequest;
        //        request.ProtocolVersion = HttpVersion.Version10;
        //    }
        //    else
        //    {
        //        request = WebRequest.Create(url) as HttpWebRequest;
        //    }
        //    request.Method = "Delete";
        //    request.ContentType = "application/x-www-form-urlencoded";
        //    request.Accept = "*/*";
        //    request.Timeout = 15000;
        //    request.AllowAutoRedirect = false;

        //    StreamWriter requestStream = null;
        //    WebResponse response = null;
        //    string responseStr = null;

        //    try
        //    {
        //        requestStream = new StreamWriter(request.GetRequestStream());
        //        requestStream.Write(param);
        //        requestStream.Close();

        //        response = request.GetResponse();
        //        if (response != null)
        //        {
        //            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
        //            responseStr = reader.ReadToEnd();
        //            reader.Close();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return responseStr;
        //}
        //private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        //{
        //    return true; //总是接受  
        //}
        //public static string BuildRequest(string strUrl, Dictionary<string, string> dicPara, string fileName)
        //{
        //    string contentType = "image/jpeg";
        //    //待请求参数数组
        //    FileStream Pic = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //    byte[] PicByte = new byte[Pic.Length];
        //    Pic.Read(PicByte, 0, PicByte.Length);
        //    int lengthFile = PicByte.Length;

        //    //构造请求地址

        //    //设置HttpWebRequest基本信息
        //    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strUrl);
        //    //设置请求方式：get、post
        //    request.Method = "POST";
        //    //设置boundaryValue
        //    string boundaryValue = DateTime.Now.Ticks.ToString("x");
        //    string boundary = "--" + boundaryValue;
        //    request.ContentType = "\r\nmultipart/form-data; boundary=" + boundaryValue;
        //    //设置KeepAlive
        //    request.KeepAlive = true;
        //    //设置请求数据，拼接成字符串
        //    StringBuilder sbHtml = new StringBuilder();
        //    foreach (KeyValuePair<string, string> key in dicPara)
        //    {
        //        sbHtml.Append(boundary + "\r\nContent-Disposition: form-data; name=\"" + key.Key + "\"\r\n\r\n" + key.Value + "\r\n");
        //    }
        //    sbHtml.Append(boundary + "\r\nContent-Disposition: form-data; name=\"pic\"; filename=\"");
        //    sbHtml.Append(fileName);
        //    sbHtml.Append("\"\r\nContent-Type: " + contentType + "\r\n\r\n");
        //    string postHeader = sbHtml.ToString();
        //    //将请求数据字符串类型根据编码格式转换成字节流
        //    Encoding code = Encoding.GetEncoding("UTF-8");
        //    byte[] postHeaderBytes = code.GetBytes(postHeader);
        //    byte[] boundayBytes = Encoding.ASCII.GetBytes("\r\n" + boundary + "--\r\n");
        //    //设置长度
        //    long length = postHeaderBytes.Length + lengthFile + boundayBytes.Length;
        //    request.ContentLength = length;

        //    //请求远程HTTP
        //    Stream requestStream = request.GetRequestStream();
        //    Stream myStream = null;
        //    try
        //    {
        //        //发送数据请求服务器
        //        requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
        //        requestStream.Write(PicByte, 0, lengthFile);
        //        requestStream.Write(boundayBytes, 0, boundayBytes.Length);
        //        HttpWebResponse HttpWResp = (HttpWebResponse)request.GetResponse();
        //        myStream = HttpWResp.GetResponseStream();
        //    }
        //    catch (WebException e)
        //    {
        //        //LogResult(e.Message);
        //        return "fail";
        //    }
        //    finally
        //    {
        //        if (requestStream != null)
        //        {
        //            requestStream.Close();
        //        }
        //    }

        //    //读取处理结果
        //    StreamReader reader = new StreamReader(myStream, code);
        //    StringBuilder responseData = new StringBuilder();

        //    String line;
        //    while ((line = reader.ReadLine()) != null)
        //    {
        //        responseData.Append(line);
        //    }
        //    myStream.Close();
        //    Pic.Close();

        //    return responseData.ToString();
        //}
        //#endregion

        //#region post zyp王琦
        ///// <summary>
        ///// 通用方法   王琦版本
        ///// </summary>
        ///// <param name="url">请求地址</param>
        ///// <param name="Parm">参数</param>
        ///// <returns></returns>
        //public static string sendPost(string url, string Parm)
        //{
        //    HttpWebRequest req = null;
        //    HttpWebResponse rsp = null;
        //    System.IO.Stream reqStream = null;
        //    try
        //    {
        //        //Dictionary<string, string> parameters = new Dictionary<string, string>();
        //        //parameters.Add("1", "12");
        //        //parameters.Add("2", "12");

        //        req = (HttpWebRequest)WebRequest.Create(url);
        //        req.Method = "post";
        //        req.KeepAlive = false;
        //        req.ProtocolVersion = HttpVersion.Version10;
        //        req.Timeout = 30000;
        //        req.ContentType = "application/json;charset=utf-8";
        //        //req.Headers.Add("X-Gisq-Rsa-UserId", "8c6eefb34fe74d6f80d215f25afba890");//如果需要写头部信息的时候

        //        byte[] postData = Encoding.UTF8.GetBytes(Parm);
        //        //byte[] postData = Encoding.UTF8.GetBytes(BuildQuery(parameters, "utf8"));//传输的是字典参数
        //        reqStream = req.GetRequestStream();
        //        reqStream.Write(postData, 0, postData.Length);
        //        rsp = (HttpWebResponse)req.GetResponse();
        //        Encoding encoding = Encoding.GetEncoding(rsp.CharacterSet);
        //        return GetResponseAsString(rsp, encoding);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //    }
        //    finally
        //    {
        //        if (reqStream != null) reqStream.Close();
        //        if (rsp != null) rsp.Close();
        //    }
        //}
        ///// <summary>
        ///// 默认传输的是字典类型
        ///// </summary>
        ///// <param name="parameters"></param>
        ///// <param name="encode"></param>
        ///// <returns></returns>
        //public static string BuildQuery(IDictionary<string, string> parameters, string encode)
        //{
        //    StringBuilder postData = new StringBuilder();
        //    bool hasParam = false;
        //    IEnumerator<KeyValuePair<string, string>> dem = parameters.GetEnumerator();
        //    while (dem.MoveNext())
        //    {
        //        string name = dem.Current.Key;
        //        string value = dem.Current.Value;
        //        // 忽略参数名或参数值为空的参数
        //        if (!string.IsNullOrEmpty(name))//&& !string.IsNullOrEmpty(value)
        //        {
        //            if (hasParam)
        //            {
        //                postData.Append("&");
        //            }
        //            postData.Append(name);
        //            postData.Append("=");
        //            if (encode == "gb2312")
        //            {
        //                postData.Append(HttpUtility.UrlEncode(value, Encoding.GetEncoding("gb2312")));
        //            }
        //            else if (encode == "utf8")
        //            {
        //                postData.Append(HttpUtility.UrlEncode(value, Encoding.UTF8));
        //            }
        //            else
        //            {
        //                postData.Append(value);
        //            }
        //            hasParam = true;
        //        }
        //    }
        //    return postData.ToString();
        //}
        //public static string GetResponseAsString(HttpWebResponse rsp, Encoding encoding)
        //{
        //    System.IO.Stream stream = null;
        //    StreamReader reader = null;
        //    try
        //    {
        //        // 以字符流的方式读取HTTP响应
        //        stream = rsp.GetResponseStream();
        //        reader = new StreamReader(stream, encoding);
        //        return reader.ReadToEnd();
        //    }
        //    finally
        //    {
        //        // 释放资源
        //        if (reader != null) reader.Close();
        //        if (stream != null) stream.Close();
        //        if (rsp != null) rsp.Close();
        //    }
        //}

        //#endregion

        //#region 参数排序
        ///// <summary>
        ///// 参数排序
        ///// </summary>
        ///// <param name="parames"></param>
        ///// <returns></returns>
        //public static Tuple<string, string> GetSortString(Dictionary<string, string> parames)
        //{
        //    // 第一步：把字典按Key的字母顺序排序
        //    IDictionary<string, string> sortedParams = new SortedDictionary<string, string>(parames);
        //    IEnumerator<KeyValuePair<string, string>> dem = sortedParams.GetEnumerator();

        //    // 第二步：把所有参数名和参数值串在一起
        //    StringBuilder query = new StringBuilder("");  //签名字符串
        //    StringBuilder queryStr = new StringBuilder(""); //url参数
        //    if (parames == null || parames.Count == 0)
        //        return new Tuple<string, string>("", "");

        //    while (dem.MoveNext())
        //    {
        //        string key = dem.Current.Key;
        //        string value = dem.Current.Value;
        //        if (!string.IsNullOrEmpty(key))
        //        {
        //            query.Append(key).Append(value);
        //            queryStr.Append("&").Append(key).Append("=").Append(value);
        //        }
        //    }
        //    return new Tuple<string, string>(query.ToString(), queryStr.ToString().Substring(1, queryStr.Length - 1));
        //}
        //#endregion

        //#region 获取时间戳
        ///// <summary>
        ///// 获取时间戳
        ///// </summary>
        ///// <returns></returns>
        //private static string GetTimeStamp()
        //{
        //    TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        //    return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        //}
        //#endregion

        //#region 获取随机数
        ///// <summary>
        /////获取随机数  
        ///// </summary>
        ///// <returns></returns>
        //private static string GetRandom()
        //{
        //    Random rd = new Random(DateTime.Now.Millisecond);
        //    int i = rd.Next(0, int.MaxValue);
        //    return i.ToString();
        //}
        //#endregion


        //#region DIY

        ///// <summary>
        ///// post传递form-data文件 
        ///// </summary>
        ///// <param name="url">请求地址</param>
        ///// <param name="stream">文件流</param>
        ///// <param name="parameter">参数</param>
        ///// <remarks>2020年5月23日11:44:04</remarks>
        ///// <returns></returns>
        //public static async Task<T> PostFormData<T>(string url, Stream stream, string parameter)
        //{
        //    using HttpClient client = new HttpClient();
        //    var formData = new MultipartFormDataContent();
        //    byte[] data = new byte[0];
        //    using (var br = new BinaryReader(stream))
        //    {
        //        data = br.ReadBytes((int)stream.Length);
        //    }
        //    var byteContent = new ByteArrayContent(data);
        //    byteContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
        //    {
        //        Name = parameter,
        //        FileName = parameter
        //    };
        //    formData.Add(byteContent);

        //    var response = await client.PostAsync(url, formData);
        //    return await ConvertResponseResult<T>(response);
        //}

        //private static Task<T> ConvertResponseResult<T>(HttpResponseMessage httpResponse)
        //{
        //    if (httpResponse.StatusCode == HttpStatusCode.OK)
        //        return null;

        //    var resStr = httpResponse.Content.ReadAsStringAsync().Result;
        //    return Task.FromResult(JsonConvert.DeserializeObject<T>(resStr));
        //}

        //#endregion
    }
}
