using Azrng.AspNetCore.Inject.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace InjectSample.Application
{
    [InjectOn(ServiceLifetime.Scoped)] // 自动注册
    public class UserService : IUserService
    {
        public string GetName(string id)
        {
            return id + "name";
        }
    }

    public interface IUserService
    {
        string GetName(string id);
    }
}