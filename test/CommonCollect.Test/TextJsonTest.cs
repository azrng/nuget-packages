// namespace CommonCollect.Test;
//
// public class TextJsonTest
// {
//     [Fact]
//     public void SerializeClass_ReturnOk()
//     {
//         var test = new JsonTest(10, "张三");
//         var str = test.ToJson();
//         Assert.Equal(@"{""Id"":10,""Name"":""张三"",""Child"":null}", str);
//     }
//
//     [Fact]
//     public void DeserializeClass_ReturnOk()
//     {
//         var str = @"{""Id"":10,""Name"":""张三"",""Child"":null}";
//         var jsonTest = str.ToObject<JsonTest>();
//         Assert.Equal(jsonTest.Name, "张三");
//     }
//
//     [Fact]
//     public void SerializeNull_ReturnNull()
//     {
//         JsonTest test = null;
//         Assert.Null(test.ToJson());
//     }
//
//     [Fact]
//     public void DeserializeNull_ReturnNull()
//     {
//         string test = null;
//         Assert.Null(test.ToObject<JsonTest>());
//     }
// }
//
// public class JsonTest
// {
//     public JsonTest(int id, string name)
//     {
//         Id = id;
//         Name = name;
//     }
//
//     public int Id { get; set; }
//
//     public string Name { get; set; }
//
//     public JsonTest Child { get; set; }
// }