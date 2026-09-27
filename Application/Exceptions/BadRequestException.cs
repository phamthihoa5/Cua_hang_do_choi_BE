namespace Application.Exceptions;

public class BadRequestException : Exception
{
    public bool Success { get; } = false;
    public string Details { get; }

    public BadRequestException(string message, string details = "") : base(message)
    {
        Details = details;
    }
}