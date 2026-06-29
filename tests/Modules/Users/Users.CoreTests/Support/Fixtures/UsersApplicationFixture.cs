using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Gorgeous.Abstractions.Application;
using Shared.AppModel.Abstractions;
using Shared.TestKit.TestDoubles;
using Users.Application;
using Users.Domain.Repositories;
using Users.CoreTests.Support.TestDoubles;

namespace Users.CoreTests.Support.Fixtures;

internal sealed class UsersApplicationFixture : IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    public UsersApplicationFixture(DateTime nowUtc)
    {
        Clock = new FixedClock(nowUtc);
        UserRepository = new FakeUserRepository();
        RoleRepository = new FakeRoleRepository();
        UnitOfWork = new FakeUnitOfWork();

        var services = new ServiceCollection();
        services.AddUsersCore();
        services.AddSingleton<IClock>(Clock);
        services.AddSingleton<IUserRepository>(UserRepository);
        services.AddSingleton<IRoleRepository>(RoleRepository);
        services.AddSingleton<IUnitOfWork>(UnitOfWork);

        _serviceProvider = services.BuildServiceProvider();
        Sender = _serviceProvider.GetRequiredService<ISender>();
    }

    public FixedClock Clock { get; }

    public FakeUserRepository UserRepository { get; }

    public FakeRoleRepository RoleRepository { get; }

    public FakeUnitOfWork UnitOfWork { get; }

    public ISender Sender { get; }

    public void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
