using Azrng.NTika.Core.Config;
using Azrng.NTika.Core.Exception;
using Azrng.NTika.Core.Extractor;
using Azrng.NTika.Core.Model;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Core.Test.Extractor
{
    public class EmbeddedDocumentUtilTests
    {
        [Fact]
        public void PushDepth_WhenDepthLimitReached_ShouldThrow()
        {
            var context = new ParseContext();
            context.Set(new EmbeddedLimits { MaxEmbeddedDepth = 1 });

            EmbeddedDocumentUtil.PushDepth(context);

            var act = () => EmbeddedDocumentUtil.PushDepth(context);
            act.Should().Throw<EmbeddedLimitReachedException>();
        }
    }
}
