using Microsoft.Extensions.Logging;

namespace Azrng.AspNetCore.Authorization.Default;

internal static class AuthorizationEventIds
{
    internal static readonly EventId EmptyPath = new(1000, nameof(EmptyPath));
    internal static readonly EventId Unauthenticated = new(1001, nameof(Unauthenticated));
    internal static readonly EventId Denied = new(1002, nameof(Denied));
    internal static readonly EventId EvaluatorError = new(1003, nameof(EvaluatorError));
    internal static readonly EventId Allowed = new(1004, nameof(Allowed));
}
