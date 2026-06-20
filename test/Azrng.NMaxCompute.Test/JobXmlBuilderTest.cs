using System.Xml.Linq;
using Azrng.NMaxCompute.Core;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class JobXmlBuilderTest
{
    [Fact]
    public void Build_QueryWrappedInCData()
    {
        var xml = JobXmlBuilder.Build("SELECT 1 AS a", new SqlHints());

        // FormatCData 会自动追加末尾的 `;`
        Assert.Contains("<Query><![CDATA[SELECT 1 AS a;]]></Query>", xml);
    }

    [Fact]
    public void Build_RootIsInstance()
    {
        var xml = JobXmlBuilder.Build("SELECT 1", new SqlHints());

        var doc = XDocument.Parse(xml);
        Assert.Equal("Instance", doc.Root!.Name.LocalName);
    }

    [Fact]
    public void Build_HintsInjectedIntoSettings()
    {
        var hints = new SqlHints { ["odps.sql.mapper.split.size"] = "256" };
        var xml = JobXmlBuilder.Build("SELECT 1", hints);

        Assert.Contains("odps.sql.mapper.split.size", xml);
        Assert.Contains("256", xml);
    }

    [Fact]
    public void Build_SqlWithExistingSemicolon_NotDuplicated()
    {
        var xml = JobXmlBuilder.Build("SELECT 1;", new SqlHints());

        Assert.Contains("<Query><![CDATA[SELECT 1;]]></Query>", xml);
        Assert.DoesNotContain("SELECT 1;;", xml);
    }

    [Fact]
    public void Build_EmptySql_Throws()
    {
        Assert.Throws<ArgumentException>(() => JobXmlBuilder.Build("", new SqlHints()));
    }

    /// <summary>
    /// 回归：&lt;SQL&gt; 子元素顺序必须为 Name → Config → Query，否则服务端报
    /// ODPS-0420031 "Element 'Config' ... Expected is ( Condition )"。对齐 PyODPS 规范。
    /// </summary>
    [Fact]
    public void Build_SqlChildOrder_Name_Config_Query()
    {
        var xml = JobXmlBuilder.Build("SELECT 1", new SqlHints());
        var doc = XDocument.Parse(xml);

        var sql = doc.Root!.Element("Job")!.Element("Tasks")!.Element("SQL")!;
        var childNames = sql.Elements().Select(e => e.Name.LocalName).ToArray();

        Assert.Equal(new[] { "Name", "Config", "Query" }, childNames);
    }

    /// <summary>
    /// 回归：&lt;Job&gt; 在 Tasks 之后必须包含 &lt;DAG&gt;&lt;RunMode&gt;Sequence&lt;/RunMode&gt;&lt;/DAG&gt;。
    /// </summary>
    [Fact]
    public void Build_DagRunModePresent_AfterTasks()
    {
        var xml = JobXmlBuilder.Build("SELECT 1", new SqlHints());
        var doc = XDocument.Parse(xml);

        var job = doc.Root!.Element("Job")!;
        var tasks = job.Element("Tasks")!;
        var dag = job.Element("DAG");

        Assert.NotNull(dag);
        Assert.Equal("Sequence", dag!.Element("RunMode")!.Value);
        // 顺序：Tasks 在 DAG 之前
        Assert.True(tasks.NodesBeforeSelf().Count() <= dag.NodesBeforeSelf().Count());
    }
}
