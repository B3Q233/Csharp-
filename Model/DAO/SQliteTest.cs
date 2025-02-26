using System;
using System.Data.SQLite;

namespace SpiderForJobInCore.Model.DAO
{
    internal class SQliteTest
    {
        static void test()
        {
            string connectionString = "Data Source=test.db;Version=3;";
            CreateDatabaseAndTable(connectionString);
            InsertData(connectionString, "John Doe", 25);
            QueryData(connectionString);
        }

        static void CreateDatabaseAndTable(string connectionString)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Age INTEGER NOT NULL
                );";

                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        static void InsertData(string connectionString, string name, int age)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string insertQuery = "INSERT INTO Users (Name, Age) VALUES (@Name, @Age);";
                using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Age", age);
                    command.ExecuteNonQuery();
                }
            }
        }

        static void QueryData(string connectionString)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT * FROM Users;";
                using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            int age = reader.GetInt32(2);
                            Console.WriteLine($"Id: {id}, Name: {name}, Age: {age}");
                        }
                    }
                }
            }
        }
    }
}
