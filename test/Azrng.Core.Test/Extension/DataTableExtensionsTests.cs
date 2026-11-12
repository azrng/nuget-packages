using System;
using System.Data;
using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class DataTableExtensionsTests
{
    [Fact]
    public void ToEntity_WithMultipleRows_ShouldReturnFirstRow()
    {
        // 方法名为单数语义即取首行；原实现多行会静默覆盖为最后一行
        var table = new DataTable();
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add("first");
        table.Rows.Add("second");

        var entity = table.ToEntity<SampleEntity>();

        entity.Name.Should().Be("first");
    }

    [Fact]
    public void ToEntities_WithMultipleRows_ShouldReturnAllRows()
    {
        var table = new DataTable();
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add("first");
        table.Rows.Add("second");

        var entities = table.ToEntities<SampleEntity>();

        entities.Should().HaveCount(2);
        entities![0].Name.Should().Be("first");
        entities[1].Name.Should().Be("second");
    }

    [Fact]
    public void ToEntity_WithGuidColumn_ShouldParseGuid()
    {
        var guid = Guid.NewGuid();
        var table = new DataTable();
        table.Columns.Add("Id", typeof(string));
        table.Rows.Add(guid.ToString());

        var entity = table.ToEntity<SampleEntity>();

        entity.Id.Should().Be(guid);
    }

    [Fact]
    public void ToEntity_WithEnumColumn_ShouldParseEnumByName()
    {
        var table = new DataTable();
        table.Columns.Add("Level", typeof(string));
        table.Rows.Add("High");

        var entity = table.ToEntity<SampleEntity>();

        entity.Level.Should().Be(SampleLevel.High);
    }

    private sealed class SampleEntity
    {
        public string Name { get; set; } = string.Empty;

        public Guid Id { get; set; }

        public SampleLevel Level { get; set; }
    }

    private enum SampleLevel
    {
        Low,
        High
    }
}
