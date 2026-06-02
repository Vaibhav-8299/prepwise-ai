using System.ComponentModel.DataAnnotations;

namespace PrepWiseAPI.DTOs.Auth
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
