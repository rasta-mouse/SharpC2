using TeamServer.Utilities;

namespace TeamServer.Models;

public class Credential
{
    public string Id { get; set; }
    public string Domain { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public Credential()
    {
        Id = Guid.NewGuid().ToShortGuid();
    }
}