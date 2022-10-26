namespace SharpC2.API.Request;

public sealed class CreateCredentialRequest
{
    public string Domain { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}