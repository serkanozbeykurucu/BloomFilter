using BloomFilter.Shared.Responses.Abstract;
using BloomFilter.Shared.Responses.ComplexTypes;

namespace BloomFilter.Shared.Responses.Concrete;

public class Response<T> : IServiceResponse
{
    public ResponseCode ResponseCode { get; }
    public string Message { get; }
    public T? Data { get; }
    public Response(ResponseCode responseCode)
    {
        ResponseCode = responseCode;
        Message = responseCode.ToString();
    }
    public Response(ResponseCode responseCode, string message)
    {
        ResponseCode = responseCode;
        Message = message;
    }
    public Response(ResponseCode responseCode, T data)
    {
        ResponseCode = responseCode;
        Data = data;
        Message = responseCode.ToString();
    }
    public Response(ResponseCode responseCode, T data, string message)
    {
        ResponseCode = responseCode;
        Data = data;
        Message = message;
    }
}
public class Response : IServiceResponse
{
    public ResponseCode ResponseCode { get; }
    public string Message { get; }

    public Response(ResponseCode responseCode)
    {
        ResponseCode = responseCode;
        Message = responseCode.ToString();
    }

    public Response(ResponseCode responseCode, string message)
    {
        ResponseCode = responseCode;
        Message = message;
    }
}