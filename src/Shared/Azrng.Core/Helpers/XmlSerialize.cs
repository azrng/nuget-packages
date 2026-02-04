using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// xml序列化
    /// </summary>
    public static class XmlSerialize
    {
        /// <summary>
        /// 序列化对象成xml文本
        /// </summary>
        /// <param name="obj">待序列化对象</param>
        /// <param name="encoding">编码方式（默认UTF8）</param>
        /// <returns></returns>
        public static string ToXml<T>(T obj, Encoding encoding = null)
        {
            _ = encoding ?? Encoding.UTF8;

            var serializer = new XmlSerializer(typeof(T));
            using var ms = new MemoryStream();
            var xmlSet = new XmlWriterSettings
            {
                Encoding = encoding,
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                OmitXmlDeclaration = false
            };

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            using (var xw = XmlWriter.Create(ms, xmlSet))
            {
                serializer.Serialize(xw, obj, ns);
            }

            ms.Position = 0;
            using var sw = new StreamReader(ms, encoding);
            return sw.ReadToEnd();
        }

        /// <summary>
        /// 序列化对象成xml文本，并输出到指定路径文件
        /// </summary>
        /// <param name="obj">待序列化对象</param>
        /// <param name="path">输出文件路径</param>
        /// <returns></returns>
        public static string ToXml<T>(T obj, string path)
        {
            var xml = new XmlDocument { InnerXml = ToXml(obj) };
            IOHelper.WriteXml(path, xml);

            return xml.InnerXml;
        }

        /// <summary>
        /// 反序列化xml文本至对象
        /// </summary>
        /// <param name="xmlOrPath">待反序列化文本或文件的路径</param>
        /// <param name="isPath">是否从指定路径加载文件</param>
        /// <param name="encoding">编码方式（默认UTF8）</param>
        /// <returns></returns>
        public static T XmlTo<T>(string xmlOrPath, bool isPath = false, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;

            // 如果是文件路径，则先加载文件
            var xml = isPath ? IOHelper.ReadXml(xmlOrPath).InnerXml : xmlOrPath;

            var serializer = new XmlSerializer(typeof(T));
            using var ms = new MemoryStream(encoding.GetBytes(xml));
            using var sw = new StreamReader(ms, encoding);
            return (T)serializer.Deserialize(sw);
        }
    }
}