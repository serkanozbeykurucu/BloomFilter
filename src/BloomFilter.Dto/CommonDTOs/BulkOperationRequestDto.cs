using System.ComponentModel.DataAnnotations;

namespace BloomFilter.Dto.CommonDTOs;

public class BulkOperationRequestDto
{
    [Required(ErrorMessage = "Values are required")]
    [MinLength(1, ErrorMessage = "At least one value is required")]
    public List<string> Values { get; set; } = new();

    public string? Description { get; set; }
}