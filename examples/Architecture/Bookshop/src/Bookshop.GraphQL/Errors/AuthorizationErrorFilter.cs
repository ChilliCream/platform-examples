using Bookshop.Application.Common.Authorization;
using HotChocolate.Execution;

namespace Bookshop.GraphQL.Errors;

internal sealed class AuthorizationErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
        => error.Exception is AuthorizationDeniedException denied
            ? error
                .WithMessage("The current user is not authorized to perform this operation.")
                .WithCode("AUTH_NOT_AUTHORIZED")
                .SetExtension("permission", denied.Permission)
                .WithException(null)
            : error;
}
