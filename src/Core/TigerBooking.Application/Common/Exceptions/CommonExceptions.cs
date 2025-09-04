namespace TigerBooking.Application.Common.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message) 
        : base(message, "NOT_FOUND", 404)
    {
    }

    public NotFoundException(string resourceName, object key) 
        : base($"{resourceName} with key '{key}' was not found.", "NOT_FOUND", 404)
    {
    }
}

public class BadRequestException : BaseException
{
    public BadRequestException(string message) 
        : base(message, "BAD_REQUEST", 400)
    {
    }
}

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message = "Unauthorized access.") 
        : base(message, "UNAUTHORIZED", 401)
    {
    }
}

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message = "Access forbidden.") 
        : base(message, "FORBIDDEN", 403)
    {
    }
}

public class ConflictException : BaseException
{
    public ConflictException(string message) 
        : base(message, "CONFLICT", 409)
    {
    }
}
