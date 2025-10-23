namespace BloomFilter.Dto.CommonDTOs;

public class BulkOperationResponseDto
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> FailedItems { get; set; } = new();
    public List<string> SuccessItems { get; set; } = new();
    public DateTime ProcessedDate { get; set; } = DateTime.UtcNow;
}