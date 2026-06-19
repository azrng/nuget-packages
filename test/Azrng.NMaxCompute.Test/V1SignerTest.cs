using Azrng.NMaxCompute.Accounts.Signers;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class V1SignerTest
{
    [Fact]
    public void BuildAuthorization_Format()
    {
        var signer = new V1Signer("LTAI5tFakeAccessId", "FakeSecretKey123");
        var auth = signer.BuildAuthorization("GET\n\n\nFri, 13 Feb 2026 09:00:00 GMT\n/projects/p");

        Assert.StartsWith("ODPS LTAI5tFakeAccessId:", auth);
    }

    [Fact]
    public void BuildAuthorization_Deterministic()
    {
        var signer = new V1Signer("AccessId", "SecretKey");
        var a = signer.BuildAuthorization("canonical");
        var b = signer.BuildAuthorization("canonical");
        Assert.Equal(a, b);
    }

    [Fact]
    public void BuildAuthorization_DifferentInputs_DifferentSignatures()
    {
        var signer = new V1Signer("AccessId", "SecretKey");
        var a = signer.BuildAuthorization("canonical-A");
        var b = signer.BuildAuthorization("canonical-B");
        Assert.NotEqual(a, b);
    }
}
