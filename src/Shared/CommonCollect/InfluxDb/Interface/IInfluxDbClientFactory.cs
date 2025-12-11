namespace CommonCollect.InfluxDb.Interface
{
    public interface IInfluxDbClientFactory
    {
        InfluxDbClientDecorator CreateClient();
    }
}
