using Apache.Arrow; using Apache.Arrow.Arrays; using Apache.Arrow.Types;
using System.Reflection;
using Xunit; using Xunit.Abstractions;
namespace Azrng.NMaxCompute.Arrow.Test;
public class ApiProbe2
{
    private readonly ITestOutputHelper _o; public ApiProbe2(ITestOutputHelper o) => _o = o;
    [Fact]
    public void P()
    {
        void Dump(Type t)
        {
            _o.WriteLine($"=== {t.FullName} ctors ===");
            foreach (var c in t.GetConstructors())
                _o.WriteLine($"  ctor({string.Join(",", c.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");
            foreach (var p in t.GetProperties(BindingFlags.Public|BindingFlags.Instance))
                _o.WriteLine($"  prop {p.PropertyType.Name} {p.Name}");
        }
        void DumpMethods(Type t)
        {
            _o.WriteLine($"=== {t.FullName} methods ===");
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                if (!m.IsSpecialName)
                    _o.WriteLine($"  {m.ReturnType.Name} {m.Name}({string.Join(",", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");
        }
        Dump(typeof(StructType)); Dump(typeof(StructArray)); Dump(typeof(TimestampArray));
        _o.WriteLine("=== ArrowBuffer static ===");
        foreach (var f in typeof(ArrowBuffer).GetFields(BindingFlags.Public | BindingFlags.Static))
            _o.WriteLine($"  field {f.FieldType.Name} {f.Name}");
        Dump(typeof(ArrowBuffer));
        DumpMethods(typeof(ArrowBuffer));
        _o.WriteLine($"ArrowBuffer is struct: {typeof(ArrowBuffer).IsValueType}");
        Dump(typeof(RecordBatch));
        // ArrowBuffer.Builder<long>：转 struct(sec,nano)→TimestampArray 需要的 buffer 构造器
        var btOpen = typeof(ArrowBuffer).GetNestedType("Builder`1");
        _o.WriteLine($"ArrowBuffer has Builder`1: {btOpen != null}");
        if (btOpen != null)
        {
            var bt = btOpen.MakeGenericType(typeof(long));
            Dump(bt); DumpMethods(bt);
        }
        // Int64/Int32Array.Values → ReadOnlySpan<long>/<int>
        foreach (var p in new[] { typeof(Int64Array).GetProperty("Values"), typeof(Int32Array).GetProperty("Values") })
            _o.WriteLine($"  {p?.DeclaringType.Name}.Values : {p?.PropertyType.Name}");
    }
}
