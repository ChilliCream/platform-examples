using Bookshop.Application.Authors;
using Bookshop.Application.Common.Authorization;

namespace Bookshop.Application.Tests.Authors;

public sealed class ListAuthorsHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_DenyBeforeLoading_When_PermissionIsMissing()
    {
        var handler = new ListAuthorsHandler(null!, new DenyAuthorizationService());
        var query = new ListAuthors(default, null!);

        var exception = await Assert.ThrowsAsync<AuthorizationDeniedException>(
            async () => await handler.HandleAsync(query, CancellationToken.None));

        Assert.Equal(Permissions.Authors.Read, exception.Permission);
    }

    private sealed class DenyAuthorizationService : IAuthorizationService
    {
        public ValueTask<bool> HasPermissionAsync(
            string permission,
            CancellationToken cancellationToken)
            => ValueTask.FromResult(false);
    }
}
