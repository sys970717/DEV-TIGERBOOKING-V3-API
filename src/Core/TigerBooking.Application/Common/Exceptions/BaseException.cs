namespace TigerBooking.Application.Common.Exceptions;

public abstract class BaseException : Exception
{
    public string ErrorCode { get; }
    public int HttpStatusCode { get; }

    protected BaseException(string message, string errorCode, int httpStatusCode) 
        : base(message)
    {
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode;
    }

    protected BaseException(string message, string errorCode, int httpStatusCode, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode;
    }
}
