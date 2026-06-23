using Shared.BuildingBlocks.Core.Domain;
using Shared.BuildingBlocks.Core.Results;
using Users.Domain.Foundation.Errors;

namespace Users.Domain.Entities;

public sealed class Role : Entity<long>
{
    private Role()
    {
    }

    private Role(string code, string name, string? description, bool isSystem, DateTime createdAtUtc)
    {
        Code = code;
        Name = name;
        Description = description;
        IsSystem = isSystem;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsSystem { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static Result<Role> Create(
        string code,
        string name,
        string? description,
        bool isSystem,
        DateTime nowUtc)
    {
        code = NormalizeCode(code);
        name = name.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<Role>(RoleErrors.CodeRequired);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Role>(RoleErrors.NameRequired);
        }

        return Result.Success(new Role(code, name, description?.Trim(), isSystem, nowUtc));
    }

    public void Update(string name, string? description, DateTime nowUtc)
    {
        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAtUtc = nowUtc;
    }

    public static string NormalizeCode(string code) => code.Trim().ToLowerInvariant();
}
