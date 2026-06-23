using System.ComponentModel.DataAnnotations;

namespace Auth.Presentation.WebApi.Requests;

internal sealed record RefreshTokenRequest(
    [property: Required]
    [property: MinLength(32)]
    string RefreshToken,

    [property: StringLength(120)]
    string? DeviceName);
