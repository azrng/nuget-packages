using Newtonsoft.Json;
using RestSharp;
using System.Reflection;

namespace CommonCollect
{
    public static class RestSharpClient
    {
        public static IRestResponse Post(string url, object data, string apiHost, Dictionary<string, string> headerList = null,
                                         string dataType = "json")
        {
            var val = new RestClient(apiHost);
            if (data.GetType().Name == "String")
            {
                data = JsonConvert.DeserializeObject<object>(data.ToString());
            }

            var val2 = new RestRequest(url, (Method)1);
            if (string.Equals(dataType, "json", StringComparison.OrdinalIgnoreCase))
            {
                val2.AddHeader("Accept", "application/json");
            }
            else if (string.Equals(dataType, "xml", StringComparison.OrdinalIgnoreCase))
            {
                val2.AddHeader("Accept", "application/xml");
            }

            if (headerList != null)
            {
                foreach (var header in headerList)
                {
                    val2.AddHeader(header.Key, header.Value);
                }
            }

            if (string.Equals(dataType, "json", StringComparison.OrdinalIgnoreCase))
            {
                val2.AddParameter("application/json", (object)JsonConvert.SerializeObject(data), (ParameterType)4);
            }
            else if (string.Equals(dataType, "xml", StringComparison.OrdinalIgnoreCase))
            {
                val2.AddXmlBody(data);
            }

            return val.Execute((IRestRequest)(object)val2);
        }

        public static IRestResponse Get(string url, string apiHost = "", Dictionary<string, string> data = null,
                                        Dictionary<string, string> headerList = null)
        {
            var val = string.IsNullOrWhiteSpace(apiHost) ? new RestClient() : new RestClient(apiHost);
            var val2 = new RestRequest(url, (Method)0);
            val2.AddHeader("Accept", "application/json");
            if (headerList != null)
            {
                foreach (var header in headerList)
                {
                    val2.AddHeader(header.Key, header.Value);
                }
            }

            if (data != null)
            {
                foreach (var datum in data)
                {
                    val2.AddQueryParameter(datum.Key, datum.Value);
                }
            }

            return val.Execute((IRestRequest)(object)val2);
        }

        public static IRestResponse FormData(string url, object data, string apiHost, Dictionary<string, byte[]> files = null,
                                             Dictionary<string, string> headerList = null)
        {
            var val = new RestClient(apiHost);
            var val2 = new RestRequest(url, (Method)1);
            val2.AlwaysMultipartFormData = true;
            var val3 = val2;
            if (headerList != null)
            {
                foreach (var header in headerList)
                {
                    val3.AddHeader(header.Key, header.Value);
                }
            }

            foreach (var item in ToMap(data))
            {
                val3.AddParameter(item.Key, (object)item.Value);
            }

            foreach (var file in files)
            {
                val3.AddFile(file.Key, file.Value, file.Value.GetRandomFileName(), (string)null);
            }

            return val.Execute((IRestRequest)(object)val3);

            static Dictionary<string, string> ToMap(object o)
            {
                var dictionary = new Dictionary<string, string>();
                var properties = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var propertyInfo in properties)
                {
                    var getMethod = propertyInfo.GetGetMethod();
                    if (getMethod != null && getMethod.IsPublic)
                    {
                        dictionary.Add(propertyInfo.Name, getMethod.Invoke(o, new object[0])!.ToString());
                    }
                }

                return dictionary;
            }
        }
    }
}