using System.ComponentModel.DataAnnotations;

namespace CareCollar.Server.DTOs;

public record SaveFcmTokenRequest(
    [Required] string Token
);
