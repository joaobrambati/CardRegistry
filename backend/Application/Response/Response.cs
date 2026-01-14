namespace Application.Response;

public class Response<T>
{
    public T? Data { get; set; }
    public bool Status { get; set; }
    public required string Mensagem { get; set; }
}
