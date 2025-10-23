using System.ComponentModel.DataAnnotations;

namespace BloomFilter.Dto.RequestDTOs;

public class CheckSuspiciousRequestDto
{
    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email address cannot exceed 255 characters")]
    public string EmailAddress { get; set; } = string.Empty;
}