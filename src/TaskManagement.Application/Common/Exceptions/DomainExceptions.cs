namespace TaskManagement.Application.Common.Exceptions;

public class NotFoundException(string name, object key)
    : Exception($"Entity '{name}' with key '{key}' was not found.");

public class ForbiddenException(string message = "You do not have permission to perform this action.")
    : Exception(message);

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }
}

public class ConflictException(string message) : Exception(message);

public class UnauthorizedException(string message = "Invalid credentials.") : Exception(message);
