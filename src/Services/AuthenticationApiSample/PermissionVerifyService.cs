using Azrng.AspNetCore.Authorization.Default;
using Azrng.Core;

namespace AuthenticationApiSample;

public class PermissionVerifyService : IPermissionVerifyService
{
    private readonly ICurrentUser _currentUser;

    public PermissionVerifyService(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public Task<bool> HasPermission(string path)
    {
        return Task.FromResult(_currentUser.UserId == "1" && path.Contains("path1"));
    }
}