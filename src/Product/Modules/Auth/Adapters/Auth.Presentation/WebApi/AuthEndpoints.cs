using Auth.Application;
using Auth.Application.Features.PasswordReset;
using Auth.Application.Features.Registration;
using Auth.Application.Features.Sessions;
using Auth.Application.Features.Sessions.Support;
using Auth.Application.Features.Verification;
using Auth.Presentation.WebApi.Requests;
using Auth.Presentation.WebApi.Responses;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Gorgeous.Web;
using Shared.Conventions;

namespace Auth.Presentation.WebApi;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthWebApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (
            RegisterWithEmailPasswordRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new RegisterWithEmailPasswordCommand(request.Email, request.Password, request.DisplayName),
                ct);

            return result.ToHttpResult(value => TypedResults.Created(
                $"/api/users/{value.UserPublicId}",
                new RegistrationResponse(value.UserPublicId, value.Email, value.EmailConfirmationRequired)));
        })
        .RequireRateLimiting(RateLimitPolicies.AuthRegister);

        group.MapPost("/login", async (
            LoginWithPasswordRequest request,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new LoginWithPasswordCommand(
                    request.Email,
                    request.Password,
                    request.DeviceName,
                    httpContext.Connection.RemoteIpAddress?.ToString()),
                ct);

            return result.ToHttpResult(ToAuthResponse);
        })
        .RequireRateLimiting(RateLimitPolicies.AuthLogin);

        group.MapPost("/refresh", async (
            RefreshTokenRequest request,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new RefreshTokenCommand(
                    request.RefreshToken,
                    request.DeviceName,
                    httpContext.Connection.RemoteIpAddress?.ToString()),
                ct);

            return result.ToHttpResult(ToAuthResponse);
        });

        group.MapPost("/logout", async (
            LogoutRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new LogoutCommand(request.RefreshToken), ct);

            return result.ToHttpResult();
        })
        .RequireAuthorization();

        group.MapPost("/confirm-email", async (
            ConfirmEmailRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ConfirmEmailCommand(request.Code), ct);

            return result.ToHttpResult();
        })
        .RequireRateLimiting(RateLimitPolicies.AuthVerificationResend);

        group.MapPost("/password-reset/request", async (
            RequestPasswordResetRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RequestPasswordResetCommand(request.Email), ct);

            return result.ToHttpResult();
        })
        .RequireRateLimiting(RateLimitPolicies.AuthPasswordReset);

        group.MapPost("/password-reset/reset", async (
            ResetPasswordRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new ResetPasswordCommand(request.Email, request.Token, request.NewPassword),
                ct);

            return result.ToHttpResult();
        })
        .RequireRateLimiting(RateLimitPolicies.AuthPasswordReset);

        return app;
    }

    private static IResult ToAuthResponse(AuthSession session)
    {
        return TypedResults.Ok(new AuthSessionResponse(
            session.AccessToken,
            session.AccessTokenExpiresAtUtc,
            session.RefreshToken,
            session.RefreshTokenExpiresAtUtc,
            session.UserPublicId));
    }
}

