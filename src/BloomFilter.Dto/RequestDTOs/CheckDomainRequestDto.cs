using System.ComponentModel.DataAnnotations;

namespace BloomFilter.Dto.RequestDTOs;

public class CheckDomainRequestDto
{
    [Required(ErrorMessage = "Domain name is required")]
    [MaxLength(255, ErrorMessage = "Domain name cannot exceed 255 characters")]
    public string DomainName { get; set; } = string.Empty;
}