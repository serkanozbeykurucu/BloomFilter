namespace BloomFilter.Dto.ResponseDTOs;

public class CheckResultResponseDto
{
    public string CheckedValue { get; set; } = string.Empty;
    public bool IsSuspicious { get; set; }
    public string CheckType { get; set; } = string.Empty;
    public bool IsExactMatch { get; set; }
    public DateTime CheckedDate { get; set; } = DateTime.UtcNow;
    public string? AdditionalInfo { get; set; }
}