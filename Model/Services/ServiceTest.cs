using SpiderForJobInCore.CommonTool;
using SpiderForJobInCore.Model.DataAttribute;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
namespace SpiderForJobInCore.Model.Services
{

    [TableName("mini_jobs")]
    public class MiniJob
    {
        [Column("mini_job_id", "INTEGER PRIMARY KEY AUTOINCREMENT")]
        [ForeignKey("jobs", "job_id")]
        public int MiniJobId { get; set; }
        public string Title { get; set; }

        [Column("description", "TEXT")]
        public int Description { get; set; }
    }

    [TableName("mini_job2")]
    public class MiniJob2
    {
        [Column("mini_job_id", "INTEGER PRIMARY KEY AUTOINCREMENT")]
        [ForeignKey("jobs", "job_id")]
        public int MiniJobId { get; set; }
        public string Title { get; set; }

        [Column("description", "TEXT")]
        public int Description { get; set; }
    }

    [TableName("jobs")]
    public class Job
    {
        [Column("job_id", "INTEGER PRIMARY KEY AUTOINCREMENT")]
        public int JobId { get; set; }
        public string Title { get; set; }

        [Column("mini_jobs", "CLASS")]
        public MiniJob MiniJob { get; set; }

        [Column("description", "TEXT")]
        [ForeignKey("Job", "job_id")]
        public string Description { get; set; }

        [Column("mini_job2", "CLASS")]
        public MiniJob2 MiniJob2 { get; set; }
    }
    //public class DatabaseHelper
    //{
    //    private readonly string _connectionString;

    //    public DatabaseHelper(string databasePath)
    //    {
    //        _connectionString = $"Data Source={databasePath};Version=3;";
    //    }

    //    public void CreateTable<T>()
    //    {
    //        CreateTable(typeof(T));
    //    }

    //    public void CreateTable(Type type)
    //    {
    //        Type otherTableName = null;
    //        string columnsDefinition = "";

    //        TableNameAttribute tableNameAttribute = type.GetCustomAttribute<TableNameAttribute>() ?? throw new InvalidOperationException($"The type {type.Name} does not have a TableNameAttribute.");
    //        string tableName = tableNameAttribute.TableName;
    //        PropertyInfo[] properties = type.GetProperties();
    //        string defaultCulumnType = "TEXT";
    //        foreach (PropertyInfo property in properties)
    //        {
    //            ColumnAttribute columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
    //            string columnName = "";
    //            string columnType = "";
    //            if (columnAttribute != null)
    //            {
    //                columnName = columnAttribute.ColumnName;
    //                columnType = columnAttribute.ColumnType;
    //                if (columnType == "CLASS")
    //                {
    //                    if (property.PropertyType.FullName == type.FullName)
    //                    {
    //                        throw new InvalidOperationException($"Cannot add a property of the same type as the class itself in table {tableName}. Property name: {property.Name}, Class name: {type.FullName}");
    //                    }
    //                    otherTableName = property.PropertyType;
    //                }
    //                else
    //                {
    //                    if (!string.IsNullOrEmpty(columnsDefinition))
    //                    {
    //                        columnsDefinition += ", ";
    //                    }
    //                    columnsDefinition += $"{columnName} {columnType}";
    //                }
    //            }
    //            else
    //            {
    //                columnName = ModelCommonTool.ConvertCamelCaseToSnakeCase(property.Name);
    //                columnType = defaultCulumnType;
    //                if (!string.IsNullOrEmpty(columnsDefinition))
    //                {
    //                    columnsDefinition += ", ";
    //                }
    //                columnsDefinition += $"{columnName} {columnType}";
    //            }
    //            ForeignKeyAttribute foreignKeyAttribute = property.GetCustomAttribute<ForeignKeyAttribute>();
    //            if (foreignKeyAttribute != null)
    //            {
    //                if (!string.IsNullOrEmpty(columnsDefinition))
    //                {
    //                    columnsDefinition += ", ";
    //                }
    //                columnsDefinition += $"FOREIGN KEY ({columnName}) REFERENCES {foreignKeyAttribute.ReferencedTableName}({foreignKeyAttribute.ReferencedColumnName})";
    //            }
    //        }

    //        if (string.IsNullOrEmpty(columnsDefinition))
    //        {
    //            throw new InvalidOperationException($"No columns found for type {type.Name}.");
    //        }

    //        string createTableQuery = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnsDefinition});";

    //        Console.WriteLine(createTableQuery);

    //        using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
    //        {
    //            connection.Open();
    //            using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
    //            {
    //                command.ExecuteNonQuery();
    //            }
    //        }
    //        if (otherTableName != null) CreateTable(otherTableName);
    //    }
    //}

    public class TableCreator
    {
        private readonly string _connectionString;
        public TableCreator(string connectionString)
        {
            _connectionString = $"Data Source={connectionString};Version=3;";
        }
        public void CreateTable<T>()
        {
            CreateTable(typeof(T));
        }
        public void CreateTable(Type type)
        {
            // 检查是否有 TableNameAttribute
            var tableNameAttribute = type.GetCustomAttribute<TableNameAttribute>();
            if (tableNameAttribute == null)
            {
                throw new InvalidOperationException($"The type {type.Name} does not have a TableNameAttribute.");
            }

            string tableName = tableNameAttribute.TableName;
            var properties = type.GetProperties();
            var columnDefinitions = new List<string>();
            var foreignKeyDefinitions = new List<string>();
            var otherTableTypes = new List<Type>();

            foreach (var property in properties)
            {
                var columnInfo = GetColumnInfo(property, type, tableName);
                if (columnInfo.IsClassType)
                {
                    otherTableTypes.Add(property.PropertyType);
                }
                else
                {
                    columnDefinitions.Add(columnInfo.Definition);
                    var foreignKeyDefinition = GetForeignKeyDefinition(property, columnInfo.ColumnName);
                    if (!string.IsNullOrEmpty(foreignKeyDefinition))
                    {
                        foreignKeyDefinitions.Add(foreignKeyDefinition);
                    }
                }
            }

            if (!columnDefinitions.Any())
            {
                throw new InvalidOperationException($"No columns found for type {type.Name}.");
            }
            foreach (var foreignKeyDefinition in foreignKeyDefinitions)
            {
                columnDefinitions.Add(foreignKeyDefinition);
            }
            string columnsDefinition = string.Join(", ", columnDefinitions);
            string createTableQuery = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnsDefinition});";

            Console.WriteLine(createTableQuery);

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            foreach (var otherTableType in otherTableTypes)
            {
                CreateTable(otherTableType);
            }
        }
        private (string Definition, string ColumnName, bool IsClassType) GetColumnInfo(PropertyInfo property, Type type, string tableName)
        {
            const string defaultColumnType = "TEXT";
            var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            string columnName;
            string columnType;
            bool isClassType = false;

            if (columnAttribute != null)
            {
                columnName = columnAttribute.ColumnName;
                columnType = columnAttribute.ColumnType;
                if (columnType == "CLASS")
                {
                    if (property.PropertyType.FullName == type.FullName)
                    {
                        throw new InvalidOperationException($"Cannot add a property of the same type as the class itself in table {tableName}. Property name: {property.Name}, Class name: {type.FullName}");
                    }
                    isClassType = true;
                    return (string.Empty, columnName, isClassType);
                }
            }
            else
            {
                columnName = ModelCommonTool.ConvertCamelCaseToSnakeCase(property.Name);
                columnType = defaultColumnType;
            }

            return ($"{columnName} {columnType}", columnName, isClassType);
        }
        private string GetForeignKeyDefinition(PropertyInfo property, string columnName)
        {
            var foreignKeyAttribute = property.GetCustomAttribute<ForeignKeyAttribute>();
            if (foreignKeyAttribute != null)
            {
                return $"FOREIGN KEY ({columnName}) REFERENCES {foreignKeyAttribute.ReferencedTableName}({foreignKeyAttribute.ReferencedColumnName})";
            }
            return string.Empty;
        }
    }


    class Program
    {
        static void Main()
        {
            string databasePath = "job_db.db";
            TableCreator dbHelper = new TableCreator(databasePath);
            dbHelper.CreateTable<Job>();
            Console.WriteLine("Table created successfully.");
        }
    }
}
