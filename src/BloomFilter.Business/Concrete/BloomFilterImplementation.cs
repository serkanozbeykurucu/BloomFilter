using System.Security.Cryptography;
using System.Text;

namespace BloomFilter.Business.Concrete;

public class BloomFilterImplementation
{
    private readonly int _size;
    private readonly int _hashFunctionCount;
    private readonly bool[] _bitArray;
    private int _elementCount;

    public BloomFilterImplementation(int size, int hashFunctionCount)
    {
        _size = size;
        _hashFunctionCount = hashFunctionCount;
        _bitArray = new bool[size];
        _elementCount = 0;
    }

    public BloomFilterImplementation(int size, int hashFunctionCount, byte[] serializedBitArray, int elementCount)
    {
        _size = size;
        _hashFunctionCount = hashFunctionCount;
        _bitArray = DeserializeBitArray(serializedBitArray, size);
        _elementCount = elementCount;
    }

    public void Add(string item)
    {
        var hashes = GetHashes(item);
        foreach (var hash in hashes)
        {
            _bitArray[hash] = true;
        }
        _elementCount++;
    }

    public bool Contains(string item)
    {
        var hashes = GetHashes(item);
        return hashes.All(hash => _bitArray[hash]);
    }

    public byte[] SerializeBitArray()
    {
        var byteCount = (_size + 7) / 8;
        var bytes = new byte[byteCount];

        for (int i = 0; i < _size; i++)
        {
            if (_bitArray[i])
            {
                bytes[i / 8] |= (byte)(1 << (i % 8));
            }
        }

        return bytes;
    }

    private bool[] DeserializeBitArray(byte[] bytes, int size)
    {
        var bitArray = new bool[size];

        for (int i = 0; i < size; i++)
        {
            bitArray[i] = (bytes[i / 8] & (1 << (i % 8))) != 0;
        }

        return bitArray;
    }

    private int[] GetHashes(string item)
    {
        var hashes = new int[_hashFunctionCount];
        var primaryHash = GetHash(item, 0);
        var secondaryHash = GetHash(item, primaryHash);

        for (int i = 0; i < _hashFunctionCount; i++)
        {
            hashes[i] = Math.Abs((primaryHash + i * secondaryHash) % _size);
        }

        return hashes;
    }

    private int GetHash(string item, int seed)
    {
        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes(item + seed);
        var hashBytes = sha256.ComputeHash(inputBytes);

        return BitConverter.ToInt32(hashBytes, 0);
    }

    public int elementCount => _elementCount;
    public int size => _size;
    public int hashFunctionCount => _hashFunctionCount;

    public double GetCurrentFalsePositiveRate()
    {
        if (_elementCount == 0) return 0;

        var ratio = (double)_elementCount / _size;
        return Math.Pow(1 - Math.Exp(-_hashFunctionCount * ratio), _hashFunctionCount);
    }
}