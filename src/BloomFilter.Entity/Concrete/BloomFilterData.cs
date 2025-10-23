using System.ComponentModel.DataAnnotations;

namespace BloomFilter.Entity.Concrete;

public class BloomFilterData : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string FilterName { get; set; } = string.Empty;

    [Required]
    public int FilterSize { get; set; }

    [Required]
    public int HashFunctionCount { get; set; }

    [Required]
    public byte[] BitArray { get; set; } = Array.Empty<byte>();

    public int ElementCount { get; set; } = 0;

    public double ExpectedFalsePositiveRate { get; set; } = 0.01;

    public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Description { get; set; }
}