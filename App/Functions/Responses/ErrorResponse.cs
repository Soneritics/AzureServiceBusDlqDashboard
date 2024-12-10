namespace Backend.Functions.Responses;

public class ErrorResponse(string error)
{
    public string Error { get; set; } = error;
}