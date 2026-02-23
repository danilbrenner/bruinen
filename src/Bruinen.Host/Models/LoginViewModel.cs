using System.ComponentModel.DataAnnotations;

namespace Bruinen.Host.Models;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Username")]
    public string Username { get; init; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; init; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; init; }
}

