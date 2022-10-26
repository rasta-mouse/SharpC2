using System.Text;

using TeamServer.Models;
using TeamServer.Utilities;

namespace TeamServer.Modules;

public class CoreModule : ServerModule
{
    public override byte Module => 0x01;

    public override async Task Execute(DroneTaskOutput output)
    {
        // get the task record
        var task = await Tasks.GetTask(output.TaskId);

        // update the task
        task.UpdateTask(output);

        // update db
        await Tasks.UpdateTask(task);

        // notify hub
        await Hub.Clients.All.NotifyDroneTaskUpdated(task.DroneId, task.TaskId);

        // scrape creds from output
        var parsed = CredentialParser.ParseCredentials(Encoding.UTF8.GetString(output.Output)).ToArray();
        
        // return if none
        if (!parsed.Any())
            return;

        // remove dups
        var list = new List<Credential>();
        foreach (var p in parsed)
        {
            if (list.Any(c => c.Domain.Equals(p.Domain) && c.Username.Equals(p.Username) && c.Password.Equals(p.Password)))
                continue;
            
            list.Add(p);
        }
        
        // get existing creds
        var existing = (await Credentials.GetCredentials()).ToArray();

        foreach (var cred in list)
        {
            // continue if cred already exists
            if (existing.Any(c =>
                    c.Domain.Equals(cred.Domain) && c.Username.Equals(cred.Username) &&
                    c.Password.Equals(cred.Password)))
                continue;

            // add to db
            await Credentials.AddCredential(cred);
            
            // notify hub
            await Hub.Clients.All.NotifyCredentialAdded(cred.Id);
        }
    }
}