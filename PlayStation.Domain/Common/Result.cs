namespace PlayStation.Domain.Common;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public T? Data { get; private set; }
    public List<string> Errors { get; private set; } = new();

    public static Result<T> Success(T data, string message = "Operation completed successfully")
    {
        return new Result<T> { IsSuccess = true, Data = data, Message = message };
    }

    public static Result<T> Failure(string message)
    {
        return new Result<T> { IsSuccess = false, Message = message, Errors = new List<string> { message } };
    }

    public static Result<T> Failure(List<string> errors)
    {
        return new Result<T> { IsSuccess = false, Message = string.Join(", ", errors), Errors = errors };
    }
}

public class Result
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public List<string> Errors { get; private set; } = new();

    public static Result Success(string message = "Operation completed successfully")
    {
        return new Result { IsSuccess = true, Message = message };
    }

    public static Result Failure(string message)
    {
        return new Result { IsSuccess = false, Message = message, Errors = new List<string> { message } };
    }

    public static Result Failure(List<string> errors)
    {
        return new Result { IsSuccess = false, Message = string.Join(", ", errors), Errors = errors };
    }
}
