using SQLite;

using TeamServer.Interfaces;
using TeamServer.Storage;

namespace TeamServer.Services;

public class DatabaseService : IDatabaseService
{
    private readonly SQLiteConnection _connection;
    private readonly SQLiteAsyncConnection _asyncConnection;

    public DatabaseService()
    {
        var directory = Path.Combine(Directory.GetCurrentDirectory(), "Data");

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        
        var path = Path.Combine(directory, "SharpC2.db");

        using (var conn = new SQLiteConnection(path))
        {
            conn.CreateTable<UserAuthDao>();
            conn.CreateTable<HttpHandlerDao>();
            conn.CreateTable<SmbHandlerDao>();
            conn.CreateTable<TcpHandlerDao>();
            conn.CreateTable<ExtHandlerDao>();
            conn.CreateTable<WebLogDao>();
            conn.CreateTable<HostedFileDao>();
            conn.CreateTable<CryptoDao>();
            conn.CreateTable<DroneDao>();
            conn.CreateTable<TaskRecordDao>();
            conn.CreateTable<ReversePortForwardDao>();
            conn.CreateTable<SocksDao>();
        }
        
        // open connections
        _connection = new SQLiteConnection(path);
        _asyncConnection = new SQLiteAsyncConnection(path);
    }

    public SQLiteConnection GetConnection()
        => _connection;

    public SQLiteAsyncConnection GetAsyncConnection()
        => _asyncConnection;
}