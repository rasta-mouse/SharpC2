using SQLite;

namespace TeamServer.Interfaces;

public interface IDatabaseService
{
    SQLiteConnection GetConnection();
    SQLiteAsyncConnection GetAsyncConnection();
}