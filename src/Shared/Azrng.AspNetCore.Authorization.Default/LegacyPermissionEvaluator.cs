namespace Azrng.AspNetCore.Authorization.Default;

internal sealed class LegacyPermissionEvaluator : IPermissionEvaluator
{
    private readonly IPermissionVerifyService _legacyService;

    public LegacyPermissionEvaluator(IPermissionVerifyService legacyService)
    {
        _legacyService = legacyService;
    }

    public async Task<AuthorizationDecision> AuthorizeAsync(
        PermissionContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var allowed = await _legacyService.HasPermission(context.Path);
        return allowed ? AuthorizationDecision.Allow() : AuthorizationDecision.Deny();
    }
}
