namespace SharpC2.API.Response;

public sealed class AuthenticationResponse
{
    public bool Success { get; set; }
    public string Token { get; set; }
}