using BloomFilter.Shared.Responses.ComplexTypes;

namespace BloomFilter.Shared.Responses.Abstract;

public interface IServiceResponse
{
    ResponseCode ResponseCode { get; }
    string Message { get; }
}