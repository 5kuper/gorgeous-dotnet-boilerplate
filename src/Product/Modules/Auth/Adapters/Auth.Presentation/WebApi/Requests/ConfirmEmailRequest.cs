using System.ComponentModel.DataAnnotations;

namespace Auth.Presentation.WebApi.Requests;

internal sealed record ConfirmEmailRequest(
    [property: Required]
    [property: StringLength(4096, MinimumLength = 16)]
    string Code);
