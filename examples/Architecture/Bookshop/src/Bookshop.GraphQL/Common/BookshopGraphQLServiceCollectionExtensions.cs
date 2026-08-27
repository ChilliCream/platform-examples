using Bookshop.GraphQL.Errors;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Bookshop.GraphQL;

/// <summary>
/// Wires the Bookshop GraphQL API (HotChocolate schema over Mocha mediator dispatch, reads
/// through Application-defined DataLoader interfaces and mediator queries whose handlers live in
/// Application and whose data access lives in Infrastructure) into dependency injection and
/// endpoint routing. Modeled on
/// ChilliCream.Operations.GraphQL.OperationsGraphQLServiceCollectionExtensions
/// (kot/cloud Operations.Host), minus a persisted-operations pipeline.
/// </summary>
public static class BookshopGraphQLServiceCollectionExtensions
{
    public static IRequestExecutorBuilder AddBookshopGraphQL(this IServiceCollection services)
        => services
            .AddGraphQLServer()
            .AddBookshopTypes()
            .AddGlobalObjectIdentification()
            .AddPagingArguments()
            .AddQueryContext()
            .AddMutationConventions()
            .AddErrorFilter<AuthorizationErrorFilter>()
            .ModifyPagingOptions(options =>
            {
                options.DefaultPageSize = 50;
                options.MaxPageSize = 100;
                options.RequirePagingBoundaries = false;
            });

    /// <summary>
    /// Maps the Bookshop GraphQL endpoint at <c>/graphql</c>. Authorization is enforced inside
    /// Application command/query handlers so the same rules apply to every transport.
    /// </summary>
    public static IEndpointConventionBuilder MapBookshopGraphQL(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGraphQL("/graphql");
}
