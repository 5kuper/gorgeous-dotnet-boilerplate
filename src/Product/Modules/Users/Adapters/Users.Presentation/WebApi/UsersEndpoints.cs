using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.BuildingBlocks.Application.Abstractions;
using Shared.BuildingBlocks.Core.Results;
using Shared.WebFramework;
using Users.Application.Features.Administration;
using Users.Application.Features.Profile;
using Users.Application.Features.Roles;
using Users.Presentation.WebApi.Requests;

namespace Users.Presentation.WebApi;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersWebApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .RequireAuthorization();

        group.MapGet("/me", async (ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
        {
            if (currentUser.PublicUserId is not Guid publicId)
            {
                return EndpointResults.ToProblem(new Error(
                    "Users.InvalidCurrentUser",
                    "Current user identity is invalid.",
                    ErrorType.Unauthorized));
            }

            var result = await sender.Send(new GetUserProfileQuery(publicId), ct);

            return result.ToHttpResult();
        });

        group.MapPatch("/me", async (
            UpdateCurrentUserProfileRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (currentUser.PublicUserId is not Guid publicId)
            {
                return EndpointResults.ToProblem(new Error(
                    "Users.InvalidCurrentUser",
                    "Current user identity is invalid.",
                    ErrorType.Unauthorized));
            }

            var result = await sender.Send(new UpdateProfileCommand(publicId, request.DisplayName), ct);

            return result.ToHttpResult();
        });

        var admin = group.MapGroup("/admin")
            .RequireAuthorization(AuthorizationPolicies.Admin);

        admin.MapPost("/roles", async (
            CreateRoleRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new CreateRoleCommand(request.Code, request.Name, request.Description, request.IsSystem),
                ct);

            return result.ToHttpResult(roleId => TypedResults.Created($"/api/users/admin/roles/{roleId}", new { roleId }));
        });

        admin.MapPut("/{publicId:guid}/roles/{roleCode}", async (
            Guid publicId,
            string roleCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (currentUser.PublicUserId is not Guid assignedByUserPublicId)
            {
                return EndpointResults.ToProblem(new Error(
                    "Users.InvalidCurrentUser",
                    "Current user identity is invalid.",
                    ErrorType.Unauthorized));
            }

            var result = await sender.Send(new AssignRoleCommand(publicId, roleCode, assignedByUserPublicId), ct);

            return result.ToHttpResult();
        });

        admin.MapDelete("/{publicId:guid}/roles/{roleCode}", async (
            Guid publicId,
            string roleCode,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RevokeRoleCommand(publicId, roleCode), ct);

            return result.ToHttpResult();
        });

        admin.MapPost("/{publicId:guid}/suspend", async (
            Guid publicId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new SuspendUserCommand(publicId), ct);

            return result.ToHttpResult();
        });

        return app;
    }
}

