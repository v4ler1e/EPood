namespace EPood.Application.Common;

public class OperationResult<T>
{
    public bool Success { get; set; }

    public string? Error { get; set; }

    public T? Value { get; set; }

    public static OperationResult<T> Ok(T value)
    {
        return new OperationResult<T>
        {
            Success = true,
            Value = value
        };
    }

    public static OperationResult<T> Fail(string error)
    {
        return new OperationResult<T>
        {
            Success = false,
            Error = error
        };
    }
}
