namespace TeacherGroupsManager.Shared.Results;

public class OperationResult
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public static OperationResult Success(string message = "تم تنفيذ العملية بنجاح") => new() { Succeeded = true, Message = message };
    public static OperationResult Failure(params string[] errors) => new() { Succeeded = false, Message = "تعذر تنفيذ العملية", Errors = errors };
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; init; }
    public static OperationResult<T> Success(T data, string message = "تم تنفيذ العملية بنجاح") => new() { Succeeded = true, Message = message, Data = data };
    public new static OperationResult<T> Failure(params string[] errors) => new() { Succeeded = false, Message = "تعذر تنفيذ العملية", Errors = errors };
}
