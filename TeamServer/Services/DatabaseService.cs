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
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SharpC2.db");

        using (var conn = new SQLiteConnection(path))
        {
            conn.CreateTable<UserAuthDao>();
            conn.CreateTable<HttpHandlerDao>();
            conn.CreateTable<WebLogDao>();
            conn.CreateTable<HostedFileDao>();
            conn.CreateTable<CryptoDao>();
            conn.CreateTable<DroneDao>();
            conn.CreateTable<TaskRecordDao>();
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