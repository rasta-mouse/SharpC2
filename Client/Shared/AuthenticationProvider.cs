using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

namespace Client.Shared;

public class AuthenticationProvider : AuthenticationStateProvider
{
    public async Task Login(string nick)
    {
        await SecureStorage.SetAsync("nick", nick);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    
    public void Logout()
    {
        SecureStorage.Remove("nick");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();

        var nick = await SecureStorage.GetAsync("nick");
        
        if (!string.IsNullOrWhiteSpace(nick))
        {
            var claims = new[] { new Claim(ClaimTypes.Name, nick) };
            identity = new ClaimsIdentity(claims, "Server authentication");
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}