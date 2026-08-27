using Bookshop.Application.Common.Authorization;

namespace Bookshop.Application.Tests.Common.Fakes;

public sealed class AllowAllAuthorizationService : IAuthorizationService
{
    public ValueTask<bool> HasPermissionAsync(
        string permission,
        CancellationToken cancellationToken)
        => ValueTask.FromResult(true);
}
