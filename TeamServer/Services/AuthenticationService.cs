using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.IdentityModel.Tokens;

using TeamServer.Interfaces;

namespace TeamServer.Services;

public class AuthenticationService : IAuthenticationService
{
    private string _password;
    private byte[] _jwtKey;
    
    public void SetServerPassword(string password, byte[] jwtKey)
    {
        _password = password;
        _jwtKey = jwtKey;
    }

    public bool AuthenticateUser(string nick, string password, out string token)
    {
        if (!password.Equals(_password))
        {
            token = string.Empty;
            return false;
        }

        token = GenerateToken(nick);
        return true;
    }

    private string GenerateToken(string nick)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[] { new(ClaimTypes.Name, nick) }),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_jwtKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}