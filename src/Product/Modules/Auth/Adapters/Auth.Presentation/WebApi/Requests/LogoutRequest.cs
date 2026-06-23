using System.ComponentModel.DataAnnotations;

namespace Auth.Presentation.WebApi.Requests;

internal sealed record LogoutRequest(
    [property: Required]
    [property: MinLength(32)]
    string RefreshToken);
