using BloomFilter.Entity.Enums;
using System.ComponentModel.DataAnnotations;

namespace BloomFilter.Dto.RequestDTOs;

public class UpdateReportStatusRequestDto
{
    [Required(ErrorMessage = "Report ID is required")]
    public int ReportId { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public ReportStatus Status { get; set; }

    [Required(ErrorMessage = "Reviewer name is required")]
    [MaxLength(100, ErrorMessage = "Reviewer name cannot exceed 100 characters")]
    public string ReviewedBy { get; set; } = string.Empty;
}