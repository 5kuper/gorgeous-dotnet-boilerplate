using Auth.Application;
using Auth.Application.Ports.Identity;
using Auth.Application.Ports.Messaging;
using Auth.Application.Ports.Registration;
using Auth.Application.Ports.Tokens;
using Auth.Application.Ports.Verification;
using Auth.CoreTests.Support.TestDoubles;
using Auth.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shared.BuildingBlocks.Application.Abstractions;
using Shared.BuildingBlocks.Core.Results;
using Shared.TestKit.TestDoubles;
using Users.Contracts.Authentication;
using Users.Contracts.Registration;
using Users.Contracts.Verification;

namespace Auth.CoreTests.Support.Fixtures;

internal sealed class AuthApplicationFixture : IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    public AuthApplicationFixture(DateTime nowUtc)
    {
        Clock = new FixedClock(nowUtc);
        Identity = new FakeIdentityCredentialService();
        RefreshSessions = new FakeRefreshSessionRepository();
        TokenIssuer = new FakeTokenIssuer();
        UserAuthProfiles = new FakeUserAuthProfileReader();
        RegistrationTransaction = new FakeRegistrationTransaction();
        UsersRegistration = new FakeUsersRegistration();
        EmailConfirmationProtector = new FakeEmailConfirmationCodeProtector();
        EmailConfirmationSender = new FakeEmailConfirmationSender();
        PasswordResetSender = new FakePasswordResetSender();

        var services = new ServiceCollection();
        services.AddAuthCore();
        services.AddSingleton<IClock>(Clock);
        services.AddSingleton<IIdentityCredentialService>(Identity);
        services.AddSingleton<IRefreshSessionRepository>(RefreshSessions);
        services.AddSingleton<ITokenIssuer>(TokenIssuer);
        services.AddSingleton<IUserAuthProfileReader>(UserAuthProfiles);
        services.AddSingleton<IRegistrationTransaction>(RegistrationTransaction);
        services.AddSingleton<IUsersRegistration>(UsersRegistration);
        services.AddSingleton<IEmailConfirmationCodeProtector>(EmailConfirmationProtector);
        services.AddSingleton<IEmailConfirmationSender>(EmailConfirmationSender);
        services.AddSingleton<IPasswordResetSender>(PasswordResetSender);
        UsersEmailVerification = new FakeUsersEmailVerification();
        services.AddSingleton<IUsersEmailVerification>(UsersEmailVerification);

        _serviceProvider = services.BuildServiceProvider();
        Sender = _serviceProvider.GetRequiredService<ISender>();
    }

    public FixedClock Clock { get; }

    public FakeIdentityCredentialService Identity { get; }

    public FakeRefreshSessionRepository RefreshSessions { get; }

    public FakeTokenIssuer TokenIssuer { get; }

    public FakeUserAuthProfileReader UserAuthProfiles { get; }

    public FakeRegistrationTransaction RegistrationTransaction { get; }

    public FakeUsersRegistration UsersRegistration { get; }

    public FakeEmailConfirmationCodeProtector EmailConfirmationProtector { get; }

    public FakeEmailConfirmationSender EmailConfirmationSender { get; }

    public FakePasswordResetSender PasswordResetSender { get; }

    public FakeUsersEmailVerification UsersEmailVerification { get; }

    public ISender Sender { get; }

    public void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    internal sealed class FakeUsersEmailVerification : IUsersEmailVerification
    {
        public int ConfirmEmailCalls { get; private set; }

        public Task<Result> ConfirmEmailAsync(
            Guid userPublicId,
            CancellationToken ct = default)
        {
            ConfirmEmailCalls++;

            return Task.FromResult(Result.Success());
        }
    }
}
