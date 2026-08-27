using Bookshop.Application.Common;
using Bookshop.Application.Common.Authorization;
using Bookshop.Domain.Authors;
using Mocha.Mediator;

namespace Bookshop.Application.Authors;

/// <summary>
/// Registers a new author.
/// </summary>
public sealed record AddAuthor(string Name, string? Bio) : ICommand<Guid>;

public sealed class AddAuthorHandler(
    IAuthorRepository authors,
    IUnitOfWork unitOfWork,
    IAuthorizationService authorizationService)
    : ICommandHandler<AddAuthor, Guid>
{
    public async ValueTask<Guid> HandleAsync(AddAuthor command, CancellationToken cancellationToken)
    {
        await authorizationService.EnsurePermissionAsync(Permissions.Authors.Create, cancellationToken);

        var author = Author.Create(command.Name, command.Bio);

        await authors.AddAsync(author, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return author.Id;
    }
}
