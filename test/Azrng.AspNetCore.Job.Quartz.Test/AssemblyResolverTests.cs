using Azrng.AspNetCore.Job.Quartz.Options;
using Azrng.AspNetCore.Job.Quartz.Schedules;
using FluentAssertions;
using System.Reflection;
using Xunit;

namespace Azrng.AspNetCore.Job.Quartz.Test
{
    /// <summary>
    /// AssemblyResolver 单元测试，验证 DI 注册与调度扫描共用的程序集解析逻辑
    /// </summary>
    public class AssemblyResolverTests
    {
        [Fact]
        public void Resolve_ShouldPreferExplicitAssemblies()
        {
            var explicitAsm = typeof(object).Assembly;

            var result = AssemblyResolver.Resolve(new QuartzOptions(), new[] { explicitAsm }, null, null, null);

            result.Should().Contain(explicitAsm);
        }

        [Fact]
        public void Resolve_ShouldUseEntryAssemblyByDefault()
        {
            var entry = Assembly.GetEntryAssembly();
            entry.Should().NotBeNull();

            var result = AssemblyResolver.Resolve(new QuartzOptions(), null, entry, null, null);

            result.Should().Contain(entry!);
        }

        [Fact]
        public void Resolve_ShouldApplyExactExcludedPattern()
        {
            var entry = Assembly.GetEntryAssembly()!;
            var options = new QuartzOptions
            {
                ExcludedAssemblyPatterns = new List<string> { entry.GetName().Name! }
            };

            var result = AssemblyResolver.Resolve(options, null, entry, null, null);

            result.Should().NotContain(entry);
        }

        [Fact]
        public void Resolve_ShouldApplyWildcardExcludedPattern()
        {
            var entry = Assembly.GetEntryAssembly()!;
            var entryName = entry.GetName().Name!;
            var prefix = entryName.Length > 2 ? entryName[..2] + "*" : entryName + "*";

            var options = new QuartzOptions
            {
                ExcludedAssemblyPatterns = new List<string> { prefix }
            };

            var result = AssemblyResolver.Resolve(options, null, entry, null, null);

            result.Should().NotContain(entry, $"通配符 {prefix} 应匹配 {entryName}");
        }
    }
}
