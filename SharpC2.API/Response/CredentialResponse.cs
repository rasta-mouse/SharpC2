namespace SharpC2.API.Response;

public sealed class CredentialResponse
{
    public string Id { get; set; }
    public string Domain { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}