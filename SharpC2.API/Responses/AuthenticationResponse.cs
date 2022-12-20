namespace SharpC2.API.Responses;

public sealed class AuthenticationResponse
{
    public bool Success { get; set; }
    public string Token { get; set; }
}