using System.Text;

using SharpC2.Models;

using Spectre.Console;

namespace SharpC2.Utilities;

public static class HandlerExtensions
{
    public static Table FormatTable(this IEnumerable<Handler> handlers)
    {
        var table = new Table();

        table.AddColumn("name");
        table.AddColumn("type");
        table.AddColumn("bind");
        table.AddColumn("connect");
        table.AddColumn("payload");

        foreach (var handler in handlers)
            table.AddTableRow(handler);

        return table;
    }

    private static void AddTableRow(this Table table, Handler handler)
    {
        if (handler is HttpHandler httpHandler)
        {
            table.AddRow(
                httpHandler.Name,
                httpHandler.HandlerType.ToString(),
                httpHandler.BindPort.ToString(),
                httpHandler.GetConnectRow(),
                httpHandler.PayloadType.ToString());
        }

        if (handler is SmbHandler smbHandler)
        {
            table.AddRow(
                smbHandler.Name,
                smbHandler.HandlerType.ToString(),
                smbHandler.PipeName,
                "-",
                smbHandler.PayloadType.ToString());
        }

        if (handler is TcpHandler tcpHandler)
        {
            table.AddRow(
                tcpHandler.Name,
                tcpHandler.HandlerType.ToString(),
                tcpHandler.GetBindRow(),
                "-",
                tcpHandler.PayloadType.ToString());
        }
    }

    private static string GetConnectRow(this HttpHandler handler)
    {
        var sb = new StringBuilder();
        
        sb.Append(handler.Secure ? "https://" : "http://");
        sb.Append($"{handler.ConnectAddress}:{handler.ConnectPort}");

        return sb.ToString();
    }

    private static string GetBindRow(this TcpHandler handler)
    {
        var sb = new StringBuilder();
        
        sb.Append(handler.LoopbackOnly ? "127.0.0.1" : "0.0.0.0");
        sb.Append($":{handler.BindPort}");

        return sb.ToString();
    }
}