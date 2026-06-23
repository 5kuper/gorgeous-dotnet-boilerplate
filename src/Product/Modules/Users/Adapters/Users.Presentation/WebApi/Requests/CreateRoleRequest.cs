using System.ComponentModel.DataAnnotations;

namespace Users.Presentation.WebApi.Requests;

internal sealed record CreateRoleRequest(
    [property: Required]
    [property: StringLength(64, MinimumLength = 2)]
    string Code,

    [property: Required]
    [property: StringLength(100, MinimumLength = 2)]
    string Name,

    [property: StringLength(500)]
    string? Description,

    bool IsSystem);
