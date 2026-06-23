using System.ComponentModel.DataAnnotations;

namespace Auth.Presentation.WebApi.Requests;

internal sealed record LoginWithPasswordRequest(
    [property: Required]
    [property: EmailAddress]
    [property: StringLength(254)]
    string Email,

    [property: Required]
    [property: StringLength(128, MinimumLength = 8)]
    string Password,

    [property: StringLength(120)]
    string? DeviceName);
