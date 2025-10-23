namespace BloomFilter.Shared.Responses.ComplexTypes;

public enum ResponseCode
{
    Success = 200,
    NoContent = 204,
    BadRequest = 400,
    Unauthorized = 401,
    NotFound = 404,
    Forbidden = 403,
    Fail = 500,
}