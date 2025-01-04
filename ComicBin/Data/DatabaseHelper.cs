using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicBin.Data
{
  public interface IDatabaseHelper
  {
    void InitializeDatabase();
    void AddComic(string title, string author, string releaseDate);
    List<(int Id, string Title)> GetComics();
  }

  public class DatabaseHelper : IDatabaseHelper
  {
    private readonly string _connectionString;

    public DatabaseHelper(string connectionString)
    {
      _connectionString = connectionString;
    }

    public void InitializeDatabase()
    {
      using var connection = new SqliteConnection(_connectionString);
      connection.Open();

      string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS Comics (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Author TEXT,
                ReleaseDate TEXT
            );";
      using var command = new SqliteCommand(createTableQuery, connection);
      command.ExecuteNonQuery();
    }

    public void AddComic(string title, string author, string releaseDate)
    {
      using var connection = new SqliteConnection(_connectionString);
      connection.Open();

      string insertQuery = @"
            INSERT INTO Comics (Title, Author, ReleaseDate) 
            VALUES (@Title, @Author, @ReleaseDate);";
      using var command = new SqliteCommand(insertQuery, connection);
      command.Parameters.AddWithValue("@Title", title);
      command.Parameters.AddWithValue("@Author", author);
      command.Parameters.AddWithValue("@ReleaseDate", releaseDate);
      command.ExecuteNonQuery();
    }

    public List<(int Id, string Title)> GetComics()
    {
      using var connection = new SqliteConnection(_connectionString);
      connection.Open();

      string selectQuery = "SELECT Id, Title FROM Comics;";
      using var command = new SqliteCommand(selectQuery, connection);
      using var reader = command.ExecuteReader();

      var comics = new List<(int, string)>();
      while (reader.Read())
      {
        comics.Add((reader.GetInt32(0), reader.GetString(1)));
      }
      return comics;
    }
  }
}
