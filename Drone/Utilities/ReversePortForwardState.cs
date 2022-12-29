using System;
using System.IO;
using System.Net.Sockets;

namespace Drone.Utilities;

public sealed class ReversePortForwardState : IDisposable
{
    public const int BufferSize = 1024;

    public string Id { get; }
    public TcpListener Listener { get; }
    public Socket ClientSocket { get; set; }
    public byte[] Buffer { get; set; }
    public MemoryStream Stream { get; set; }

    public ReversePortForwardState(string id, TcpListener listener)
    {
        Id = id;
        Listener = listener;
        
        Buffer = new byte[BufferSize];
        Stream = new MemoryStream();
    }

    public void WriteDataToStream(int size)
    {
        Stream.Write(Buffer, 0, size);
        Buffer = new byte[BufferSize];
    }

    public byte[] GetStreamData()
    {
        var data = Stream.ToArray();
        
        Stream.Dispose();
        Stream = new MemoryStream();

        return data;
    }

    public void DisconnectClient()
    {
        if (ClientSocket is null)
            return;
        
        ClientSocket.Dispose();
        ClientSocket = null;
    }

    public void Dispose()
    {
        DisconnectClient();
        
        Stream.Dispose();
        Listener.Stop();
    }
}