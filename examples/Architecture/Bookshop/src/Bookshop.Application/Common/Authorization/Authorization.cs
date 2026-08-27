namespace Bookshop.Application.Common.Authorization;

/// <summary>
/// Application-facing authorization seam. Implementations resolve the current principal at the
/// host boundary; handlers only ask whether that principal has a domain permission.
/// </summary>
public interface IAuthorizationService
{
    ValueTask<bool> HasPermissionAsync(
        string permission,
        CancellationToken cancellationToken);
}

public static class AuthorizationServiceExtensions
{
    public static async ValueTask EnsurePermissionAsync(
        this IAuthorizationService authorizationService,
        string permission,
        CancellationToken cancellationToken)
    {
        if (!await authorizationService.HasPermissionAsync(permission, cancellationToken))
        {
            throw new AuthorizationDeniedException(permission);
        }
    }
}

public static class Permissions
{
    public static class Authors
    {
        public const string Read = "authors:read";

        public const string Create = "authors:create";
    }

    public static class Books
    {
        public const string Read = "books:read";

        public const string Create = "books:create";

        public const string Restock = "books:restock";
    }

    public static class Customers
    {
        public const string Register = "customers:register";
    }

    public static class Orders
    {
        public const string Place = "orders:place";

        public const string Cancel = "orders:cancel";
    }
}

public sealed class AuthorizationDeniedException(string permission)
    : Exception($"The current user does not have the '{permission}' permission.")
{
    public string Permission { get; } = permission;
}
