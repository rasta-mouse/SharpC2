namespace TeamServer.Interfaces;

public interface IAuthenticationService
{
    void SetServerPassword(string password, byte[] jwtKey);
    bool AuthenticateUser(string nick, string password, out string token);
}