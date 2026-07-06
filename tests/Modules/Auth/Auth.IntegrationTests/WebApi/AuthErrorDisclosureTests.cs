using Auth.Presentation.WebApi;
using Auth.IntegrationTests.WebApi.Support;
using Gorgeous.Abstractions.Results;
using Microsoft.AspNetCore.Http;
using Shared.IntegrationTesting.Messaging;
using Shared.IntegrationTesting.WebApi;

namespace Auth.IntegrationTests.WebApi;

public sealed class AuthErrorDisclosureTests
{
    [Theory]
    [InlineData("Auth.UserNotAllowed")]
    [InlineData("Auth.EmailNotConfirmed")]
    public async Task LoginEndpoint_MasksSensitiveFailures(string errorCode)
    {
        using var app = AuthEndpointTestApp.Create(TestSender.Returning(
            Result.Failure<Auth.Application.Features.Sessions.Support.AuthSession>(
                new Error(errorCode, "Sensitive detail.", ErrorType.Forbidden))));

        var problem = await app.InvokePostProblemAsync(
            "/api/auth/login",
            """
            {
              "email": "user@example.com",
              "password": "correct-password",
              "deviceName": "browser"
            }
            """);

        problem.ShouldBeProblem(
            "Auth.LoginFailed",
            "Invalid email or password.",
            StatusCodes.Status401Unauthorized);
    }

    [Theory]
    [InlineData("Auth.RefreshSessionNotFound")]
    [InlineData("Auth.RefreshSessionExpired")]
    [InlineData("Auth.RefreshSessionReplayed")]
    public async Task RefreshEndpoint_MasksRefreshSessionFailures(string errorCode)
    {
        using var app = AuthEndpointTestApp.Create(TestSender.Returning(
            Result.Failure<Auth.Application.Features.Sessions.Support.AuthSession>(
                new Error(errorCode, "Sensitive detail.", ErrorType.Unauthorized))));

        var problem = await app.InvokePostProblemAsync(
            "/api/auth/refresh",
            """
            {
              "refreshToken": "12345678901234567890123456789012",
              "deviceName": "browser"
            }
            """);

        problem.ShouldBeProblem(
            "Auth.RefreshFailed",
            "Refresh token is invalid.",
            StatusCodes.Status401Unauthorized);
    }

    [Theory]
    [InlineData("Users.EmailAlreadyUsed")]
    [InlineData("Auth.IdentityOperationFailed")]
    public async Task RegisterEndpoint_MasksRegistrationFailures(string errorCode)
    {
        using var app = AuthEndpointTestApp.Create(TestSender.Returning(
            Result.Failure<Auth.Application.Features.Registration.RegistrationResult>(
                new Error(errorCode, "Sensitive detail.", ErrorType.Conflict))));

        var problem = await app.InvokePostProblemAsync(
            "/api/auth/register",
            """
            {
              "email": "user@example.com",
              "password": "correct-password",
              "displayName": "User Name"
            }
            """);

        problem.ShouldBeProblem(
            "Auth.RegistrationFailed",
            "Registration could not be completed.",
            StatusCodes.Status400BadRequest);
    }

    [Theory]
    [InlineData("Users.NotFound")]
    [InlineData("Auth.UserNotAllowed")]
    public async Task EmailConfirmationEndpoint_MasksConfirmationFailures(string errorCode)
    {
        using var app = AuthEndpointTestApp.Create(TestSender.Returning(
            Result.Failure(new Error(errorCode, "Sensitive detail.", ErrorType.NotFound))));

        var problem = await app.InvokePostProblemAsync(
            "/api/auth/confirm-email",
            """
            {
              "code": "1234567890123456"
            }
            """);

        problem.ShouldBeProblem(
            "Auth.EmailConfirmationFailed",
            "Email confirmation failed.",
            StatusCodes.Status400BadRequest);
    }
}
