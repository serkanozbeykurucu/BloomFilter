using BloomFilter.Entity.Enums;
using System.ComponentModel.DataAnnotations;

namespace BloomFilter.Dto.RequestDTOs;

public class CreateUserReportRequestDto
{
    [Required(ErrorMessage = "Reporter name is required")]
    [MaxLength(100, ErrorMessage = "Reporter name cannot exceed 100 characters")]
    public string ReporterName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Reporter email cannot exceed 255 characters")]
    public string? ReporterEmail { get; set; }

    [Required(ErrorMessage = "Report type is required")]
    public ReportType ReportType { get; set; }

    [Required(ErrorMessage = "Reported value is required")]
    [MaxLength(255, ErrorMessage = "Reported value cannot exceed 255 characters")]
    public string ReportedValue { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}
