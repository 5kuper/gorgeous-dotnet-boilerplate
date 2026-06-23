using System.ComponentModel.DataAnnotations;

namespace Auth.Presentation.WebApi.Requests;

internal sealed record RequestPasswordResetRequest(
    [property: Required]
    [property: EmailAddress]
    [property: StringLength(254)]
    string Email);
