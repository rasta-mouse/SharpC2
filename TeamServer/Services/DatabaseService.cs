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
        var path = Path.Combine(Directory.GetCurrentDirectory(), "SharpC2.db");

        using (var conn = new SQLiteConnection(path))
        {
            conn.CreateTable<HttpHandlerDao>();
            conn.CreateTable<TcpHandlerDao>();
            conn.CreateTable<SmbHandlerDao>();
            conn.CreateTable<DroneDao>();
            conn.CreateTable<DroneTaskRecordDao>();
            conn.CreateTable<CryptoDao>();
            conn.CreateTable<CredentialDao>();
        }
        
        // open connections
        _connection = new SQLiteConnection(path);
        _asyncConnection = new SQLiteAsyncConnection(path);
    }

    public SQLiteConnection GetConnection()
    {
        return _connection;
    }

    public SQLiteAsyncConnection GetAsyncConnection()
    {
        return _asyncConnection;
    }
}