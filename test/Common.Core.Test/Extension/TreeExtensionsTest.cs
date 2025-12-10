using Azrng.Core.Model;

namespace Common.Core.Test.Extension
{
    /// <summary>
    /// 树表达式
    /// </summary>
    public class TreeExtensionsTest
    {
        [Fact]
        public void GenerateTreeTest()
        {
            var list = new List<City>()
                       {
                           new City { Id = 1, ParentId = 0, },
                           new City { Id = 2, ParentId = 1, },
                           new City { Id = 3, ParentId = 1 },
                           new City { Id = 4, ParentId = 0, },
                           new City { Id = 5, ParentId = 4, },
                           new City { Id = 6, ParentId = 4 }
                       };
            var moduleTree = list.GenerateTree(u => u.Id, u => u.ParentId, 0).ToList();
            Assert.NotEmpty(moduleTree);
            Assert.Equal(2, moduleTree.Count);
        }

        [Fact]
        public void TraverseTest()
        {
            var list = new List<City>()
                       {
                           new City()
                           {
                               Id = 1,
                               ParentId = 0,
                               Children = new List<City> { new City { Id = 2, ParentId = 1, }, new City { Id = 3, ParentId = 1 } }
                           },
                           new City()
                           {
                               Id = 4,
                               ParentId = 0,
                               Children = new List<City> { new City { Id = 5, ParentId = 4, }, new City { Id = 6, ParentId = 4 } }
                           }
                       };

            var result = list.Traverse(t => t.Children).ToList();
            Assert.NotEmpty(result);
            Assert.True(result.Count == 6);
        }
    }

    /// <summary>
    /// 城市类
    /// </summary>
    internal class City : TreeFlatItem<int, City>
    {
        public string Name { get; set; }
    }
}