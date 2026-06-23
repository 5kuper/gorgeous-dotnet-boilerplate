using System.ComponentModel.DataAnnotations;

namespace Auth.Presentation.WebApi.Requests;

internal sealed record ResetPasswordRequest(
    [property: Required]
    [property: EmailAddress]
    [property: StringLength(254)]
    string Email,

    [property: Required]
    [property: StringLength(4096, MinimumLength = 16)]
    string Token,

    [property: Required]
    [property: StringLength(128, MinimumLength = 8)]
    string NewPassword);
