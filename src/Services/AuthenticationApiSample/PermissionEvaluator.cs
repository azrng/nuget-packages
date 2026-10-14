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
        var grantedPermissions = _currentUser.UserId == "1"
            ? new[] { "path1" }
            : Array.Empty<string>();
        var isAllowed = PermissionMetadataMatcher.IsSatisfiedBy(
            context.RequiredPermissions,
            grantedPermissions);

        return Task.FromResult(isAllowed
            ? AuthorizationDecision.Allow()
            : AuthorizationDecision.Deny());
    }
}
