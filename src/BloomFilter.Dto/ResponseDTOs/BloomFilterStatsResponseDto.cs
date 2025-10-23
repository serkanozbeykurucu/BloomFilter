namespace BloomFilter.Dto.ResponseDTOs;

public class BloomFilterStatsResponseDto
{
    public string FilterName { get; set; } = string.Empty;
    public int FilterSize { get; set; }
    public int HashFunctionCount { get; set; }
    public int ElementCount { get; set; }
    public double ExpectedFalsePositiveRate { get; set; }
    public double CurrentLoadFactor { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public string? Description { get; set; }
}