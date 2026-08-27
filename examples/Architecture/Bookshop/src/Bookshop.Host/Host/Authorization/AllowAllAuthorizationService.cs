using Bookshop.Application.Common.Authorization;

namespace Bookshop.Host.Host.Authorization;

/// <summary>
/// Keeps this unauthenticated sample host usable. A real host replaces this outer-layer adapter
/// with an implementation that evaluates the current principal's claims or policy grants.
/// </summary>
internal sealed class AllowAllAuthorizationService : IAuthorizationService
{
    public ValueTask<bool> HasPermissionAsync(
        string permission,
        CancellationToken cancellationToken)
        => ValueTask.FromResult(true);
}
