using System.ComponentModel.DataAnnotations;

namespace Auth.Presentation.WebApi.Requests;

internal sealed record RegisterWithEmailPasswordRequest(
    [property: Required]
    [property: EmailAddress]
    [property: StringLength(254)]
    string Email,

    [property: Required]
    [property: StringLength(128, MinimumLength = 8)]
    string Password,

    [property: Required]
    [property: StringLength(100, MinimumLength = 2)]
    string DisplayName);
