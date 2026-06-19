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
}
