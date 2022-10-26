using System.Text.RegularExpressions;

using TeamServer.Models;

namespace TeamServer.Utilities;

public static class CredentialParser
{
    // https://github.com/cobbr/Covenant/blob/master/Covenant/Models/Covenant/CapturedCredential.cs
    public static IEnumerable<Credential> ParseCredentials(string input)
    {
        var credentials = new List<Credential>();
        
        // Mimikatz
        if (input.Contains("mimikatz"))
        {
            var hostDomain = "";
            var domainSid = "";
            var hostName = "";

            var lines = input.Split('\n');
            
            foreach (var line in lines.Take(2))
            {
                if (!line.StartsWith("Hostname:", StringComparison.Ordinal))
                    continue;
                
                try
                {
                    var domain = string.Join(":", line.Split(":").Skip(1)).Trim();
                    var temp = domain.Split("/")[0].Trim();
                    domainSid = domain.Split("/")[1].Trim();

                    hostName = temp.Split(".")[0];
                    hostDomain = string.Join(".", temp.Split(".").TakeLast(temp.Split(".").Length - 1));
                }
                catch (Exception)
                {
                    // nothing
                }
            }

            // sekurlsa::logonpasswords
            var regexes = new List<string>
            {
                "(?s)(?<=msv :).*?(?=tspkg :)", "(?s)(?<=tspkg :).*?(?=wdigest :)",
                "(?s)(?<=wdigest :).*?(?=kerberos :)", "(?s)(?<=kerberos :).*?(?=ssp :)",
                "(?s)(?<=ssp :).*?(?=credman :)", "(?s)(?<=credman :).*?(?=Authentication Id :)",
                "(?s)(?<=credman :).*?(?=mimikatz)"
            };
            
            foreach (var regex in regexes)
            {
                var matches = Regex.Matches(input, regex);
                
                foreach (Match match in matches)
                {
                    lines = match.Groups[0].Value.Split('\n');
                    
                    var username = "";
                    var domain = "";
                    var password = "";
                    var credType = "";

                    foreach (var line in lines)
                    {
                        try
                        {
                            if (line.Contains("Username"))
                                username = string.Join(":", line.Split(":").Skip(1)).Trim();
                            else if (line.Contains("Domain"))
                                domain = string.Join(":", line.Split(":").Skip(1)).Trim();
                            else if (line.Contains("NTLM") || line.Contains("Password"))
                                password = string.Join(":", line.Split(":").Skip(1)).Trim();
                        }
                        catch (Exception)
                        {
                            // nothing
                        }
                    }

                    if (username == "" || password is "" or "(null)")
                        continue;
                    
                    var sid = "";
                    if (hostDomain.StartsWith(domain.ToLower(), StringComparison.Ordinal))
                    {
                        domain = hostDomain;
                        sid = domainSid;
                    }

                    credType = IsNtlm(password) ? "hash" : "plaintext";

                    if (credType == "plaintext" && username.EndsWith("$", StringComparison.Ordinal))
                        continue;
                    
                    if (IsNtlm(password))
                    {
                        credentials.Add(new Credential
                        {
                            Domain = domain,
                            Username = username,
                            Password = password
                        });
                    }
                    else
                    {
                        credentials.Add(new Credential
                        {
                            Domain = domain,
                            Username = username,
                            Password = password
                        });
                    }
                }
            }

            // lsadump::sam
            if (credentials.Count == 0)
            {
                if (lines.FirstOrDefault(l => l.Contains("SAMKey")) != null)
                {
                    var linesCombined = string.Join('\n', lines);
                    var domainLine = lines.FirstOrDefault(l => l.Contains("Domain :"));
                    var domain = string.Join(":", domainLine.Split(":").Skip(1)).Trim();
                    var hashMatches = Regex.Matches(linesCombined, "(?s)RID  :.*?((?=RID  :)|$)");
                    
                    foreach (Match match in hashMatches)
                    {
                        var user = "";
                        var userHash = "";
                        
                        var lines2 = match.Groups[0].Value.Split('\n').ToList();
                        
                        foreach (var line in lines2)
                        {
                            try
                            {
                                if (line.Trim().StartsWith("User :", StringComparison.Ordinal))
                                    user = string.Join(":", line.Split(":").Skip(1)).Trim();
                                else if (line.Trim().StartsWith("Hash NTLM:", StringComparison.Ordinal))
                                    userHash = string.Join(":", line.Split(":").Skip(1)).Trim();
                            }
                            catch (Exception)
                            {
                                // nothing
                            }
                        }

                        if (domain is not "" && user is not "" && userHash is not "")
                        {
                            credentials.Add(new Credential
                            {
                                Domain = domain,
                                Username = user,
                                Password = userHash
                            });
                        }
                    }
                }
            }
        }

        // Rubeus
        var userMatches = Regex.Matches(input, "(?s)UserName                 :.*?((?=UserName                 :)|$)");
        foreach (Match match in userMatches)
        {
            var lines2 = match.Groups[0].Value.Split('\n').ToList();
            var username = "";
            var domain = "";
            
            foreach (var line in lines2)
            {
                if (line.Contains("UserName"))
                    username = string.Join(":", line.Split(":").Skip(1)).Trim();
                else if (line.Contains("Domain"))
                    domain = string.Join(":", line.Split(":").Skip(1)).Trim();
            }

            var ticketMatches = Regex.Matches(match.Groups[0].Value,
                "(?s)ServiceName           :.*?((?=ServiceName              :)|$)");
            
            foreach (Match ticketMatch in ticketMatches)
            {

                var lines3 = ticketMatch.Groups[0].Value.Split('\n').ToList();
                var serviceName = "";
                var ticket = "";
                var keyType = "";

                foreach (var line in lines3)
                {
                    try
                    {
                        if (line.Contains("ServiceName"))
                            serviceName = string.Join(":", line.Split(":").Skip(1)).Trim();
                        else if (line.Contains("SessionKeyType"))
                            keyType = string.Join(":", line.Split(":").Skip(1)).Trim();
                        else if (line.Contains("Base64EncodedTicket"))
                            ticket = ticketMatch.Groups[0].Value
                                .Substring(ticketMatch.Groups[0].Value
                                    .IndexOf("Base64EncodedTicket", StringComparison.Ordinal) + 26).Trim()
                                .Replace(" ", "").Replace("\r", "").Replace("\n", "");
                    }
                    catch (Exception)
                    {
                        // nothing
                    }
                }

                if (serviceName != "" && ticket != "")
                {
                    credentials.Add(new Credential
                    {
                        Domain = domain,
                        Username = username,
                        Password = ticket
                    });
                }
            }
        }

        return credentials;
    }

    private static bool IsNtlm(string input)
        => Regex.IsMatch(input, "^[0-9a-f]{32}", RegexOptions.IgnoreCase);
}