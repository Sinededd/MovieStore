namespace MovieStore.Domain.Models;

public class ResponseData<T>
{
    public T? Data { get; set; }
    public bool Successfull { get; set; } = true;
    public string? ErrorMessage { get; set; }

    public static ResponseData<T> Success(T data) => new() { Data = data };

    public static ResponseData<T> Error(string message, T? data = default) =>
        new() { ErrorMessage = message, Successfull = false, Data = data };
}
