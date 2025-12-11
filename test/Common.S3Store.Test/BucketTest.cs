using Azrng.S3Store;

namespace Common.S3Store.Test;

public class BucketTest
{
    private readonly IStore _store;

    public BucketTest(IStore store)
    {
        _store = store;
    }

    /// <summary>
    /// 桶不存在就创建
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task BucketNotExistCreate_ReturnOk()
    {
        var bucketName = Guid.NewGuid().ToString();
        var flag = await _store.CheckBucketExistAsync(bucketName);
        Assert.True(!flag);

        await _store.CreateBucketIfNotExistsAsync(bucketName);
        flag = await _store.CheckBucketExistAsync(bucketName);
        Assert.True(flag);

        await Task.Delay(2000);
        await _store.DeleteBucketAsync(bucketName);
    }

    [Fact]
    public async Task CreateBucket_Ok()
    {
        var bucketName = "cesshi";
        var exist = await _store.CheckBucketExistAsync(bucketName);
        Assert.True(!exist);

        var create = await _store.CreateBucketIfNotExistsAsync(bucketName);
        Assert.True(create);

        exist = await _store.CheckBucketExistAsync(bucketName);
        Assert.True(exist);
        await _store.DeleteBucketAsync(bucketName);
    }

    [Fact]
    public async Task DeleteBucket_ReturnOk()
    {
        var bucketName = "bucketexisttest";

        // 检查桶不存在
        var flag = await _store.CheckBucketExistAsync(bucketName);
        if (flag)
        {
            // 删除桶
            await _store.DeleteBucketAsync(bucketName);
        }

        // 创建桶
        await _store.CreateBucketIfNotExistsAsync(bucketName);

        // 检查桶存在
        flag = await _store.CheckBucketExistAsync(bucketName);
        Assert.True(flag);

        // 删除桶
        await _store.DeleteBucketAsync(bucketName);

        // 检查不存在
        flag = await _store.CheckBucketExistAsync(bucketName);
        Assert.True(!flag);
    }

    [Fact]
    public async Task SetPolicy()
    {
        await _store.ConfigBucketReadAndWritePolicyAsync("aaa");
    }

    [Fact]
    public async Task GetPolicy()
    {
        await _store.GetBucketPolicyAsync("azrng");
    }
}