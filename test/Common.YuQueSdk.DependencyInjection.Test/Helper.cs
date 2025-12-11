namespace Common.YuQueSdk.DependencyInjection.Test;

public class Helper
{
    /// <summary>
    /// 根据md文件解析yaml配置
    /// </summary>
    /// <param name="markdown"></param>
    /// <returns></returns>
    public Dictionary<string, List<string>> AnalysizYaml(string markdown)
    {
        var start = markdown.IndexOf("---") + 3;
        var end = markdown.IndexOf("---", start);
        var yaml = markdown.Substring(start, end - start).Trim();
        var dic = new Dictionary<string, List<string>>();
        if (yaml.Length == 0)
            return dic;

        var configArray = yaml.Split(Environment.NewLine);
        var lastKey = string.Empty;
        for (var i = 0; i < configArray.Length; i++)
        {
            var firstSemicolonIndex = configArray[i].IndexOf(":");
            if (firstSemicolonIndex != -1)
            {
                // 添加类型：  config_ui_type: 6
                var currConfig = configArray[i].Replace(" ", "");
                var key = currConfig.Substring(0, firstSemicolonIndex);
                var value = currConfig.Substring(firstSemicolonIndex + 1, currConfig.Length - 1 - firstSemicolonIndex);
                if (dic.ContainsKey(key) && string.IsNullOrWhiteSpace(value))
                {
                    dic[key].Add(value);
                }
                else
                {
                    dic.Add(key, string.IsNullOrWhiteSpace(value) ? new List<string>() : new List<string> { value });
                }

                lastKey = key;
            }
            else
            {
                // 添加类型  - 住院时间轴
                var currValue = configArray[i].Replace(" ", "").Replace("-", "");
                if (dic.ContainsKey(lastKey) && !string.IsNullOrWhiteSpace(currValue))
                {
                    dic[lastKey].Add(currValue);
                }
            }
        }
        return dic;
    }
}