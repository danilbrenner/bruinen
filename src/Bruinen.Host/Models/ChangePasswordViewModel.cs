using System.ComponentModel.DataAnnotations;

namespace Bruinen.Host.Models;

public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; init; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; init; } = string.Empty;
}

