namespace Common.HttpClients.Next.Test;

public class HttpClientOptionsTests
{
    [Fact]
    public void PropertyNameCaseInsensitive_ShouldDefaultToTrue()
    {
        Assert.True(new HttpClientOptions().PropertyNameCaseInsensitive);
    }
}
