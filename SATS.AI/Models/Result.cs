namespace SATS.AI.Models;

public sealed record Result<T>
{
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public bool IsSuccess { get; }

    private Result(T? value, string? errorMessage, bool isSuccess)
    {
        Value = value;
        ErrorMessage = errorMessage;
        IsSuccess = isSuccess;
    }

    public static Result<T> Success(T value) => new(value, null, true);
    public static Result<T> Failure(string errorMessage) => new(default, errorMessage, false);
}
