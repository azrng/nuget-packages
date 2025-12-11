using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Common.Results.Lay
{
    /// <summary>
    /// Select2树形下拉列表模型。
    /// </summary>
    public class LayTreeSelect
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("parentId")]
        public string ParentId { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }

    public static class LayTreeSelectHelper
    {
        public static string ToTreeSelectJson(this List<LayTreeSelect> data, string parentId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(ToTreeSelectJson(data, parentId, ""));
            sb.Append("]");
            return sb.ToString();
        }

        private static string ToTreeSelectJson(List<LayTreeSelect> data, string parentId, string blank)
        {
            StringBuilder sb = new StringBuilder();
            var childList = data.FindAll(t => t.ParentId == parentId);

            var tabline = "";
            if (parentId != "0")
            {
                tabline = "　　";
            }
            if (childList.Count > 0)
            {
                tabline += blank;
            }
            foreach (LayTreeSelect entity in childList)
            {
                entity.Text = tabline + entity.Text;
                string strJson = JsonConvert.SerializeObject(entity);
                sb.Append(strJson);
                sb.Append(ToTreeSelectJson(data, entity.Id, tabline));
            }
            return sb.ToString().Replace("}{", "},{");
        }

        /// <summary>
        ///拼接装list
        /// </summary>
        /// <param name="data"></param>
        /// <param name="parentId"></param>
        /// <param name="blank"></param>
        /// <returns></returns>
        public static List<LayTreeSelect> ToTreeSelectList(this List<LayTreeSelect> data, string parentId, string blank)
        {
            var list = new List<LayTreeSelect>();
            var childList = data.FindAll(t => t.ParentId == parentId);
            var tabline = "";
            if (parentId != "0")
            {
                tabline = "　　";
            }
            if (childList.Count > 0)
            {
                tabline += blank;
            }
            foreach (var entity in childList)
            {
                entity.Text = tabline + entity.Text;
                list.Add(entity);
                list.AddRange(data.ToTreeSelectList(entity.Id, tabline));
            }
            return list;
        }
    }
}