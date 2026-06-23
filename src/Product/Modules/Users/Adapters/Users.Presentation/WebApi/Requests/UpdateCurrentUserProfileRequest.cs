using System.ComponentModel.DataAnnotations;

namespace Users.Presentation.WebApi.Requests;

internal sealed record UpdateCurrentUserProfileRequest(
    [property: Required]
    [property: StringLength(100, MinimumLength = 2)]
    string DisplayName);
