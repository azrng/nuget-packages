namespace Common.YuQueSdk.DependencyInjection.Test.Model;

/// <summary>
/// vuepress菜单模型
/// </summary>
public class VuepressMenuDto
{
    public VuepressMenuDto(string text)
    {
        this.text = text;
        this.children = new List<VuepressMenuDto>();
    }

    public VuepressMenuDto(string text, string prefix)
    {
        this.text = text;
        this.prefix = prefix;
        this.children = new List<VuepressMenuDto>();
    }

    /// <summary>
    /// 标题
    /// </summary>
    public string text { get; set; }

    /// <summary>
    /// 相对路径前缀
    /// </summary>
    public string prefix { get; set; }

    /// <summary>
    /// 是否收起
    /// </summary>
    public bool collapsible { get; set; } = true;

    /// <summary>
    /// 子项目
    /// </summary>
    public List<VuepressMenuDto> children { get; set; }

    public void AddChild(VuepressMenuDto vuepressMenuDto)
    {
        children.Add(vuepressMenuDto);
    }
}