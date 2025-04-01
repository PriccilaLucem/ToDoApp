using System.Text.Json;

namespace WebApplication.Src.Config;
public class ErrorDetails
{
    public required int StatusCode { get; set; }
    public required string Message { get; set; }
    public required string Details { get; set; }
    public  override string ToString() => JsonSerializer.Serialize(this);

}