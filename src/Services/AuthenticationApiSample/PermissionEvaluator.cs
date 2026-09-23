using Azrng.AspNetCore.Authorization.Default;
using Azrng.Core;

namespace AuthenticationApiSample;

public class PermissionEvaluator : IPermissionEvaluator
{
    private readonly ICurrentUser _currentUser;

    public PermissionEvaluator(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public Task<AuthorizationDecision> AuthorizeAsync(
        PermissionContext context,
        CancellationToken cancellationToken = default)
    {
        var isAllowed = _currentUser.UserId == "1"
            && context.Path.Contains("path1", StringComparison.OrdinalIgnoreCase);

        return Task.FromResult(isAllowed
            ? AuthorizationDecision.Allow()
            : AuthorizationDecision.Deny());
    }
}
