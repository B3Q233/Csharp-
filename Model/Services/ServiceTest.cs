using SpiderForJobInCore.CommonTool;
using SpiderForJobInCore.Model.DAO;
using SpiderForJobInCore.Model.DataAttribute;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
namespace SpiderForJobInCore.Model.Services
{
    [TableName("test")]
    public class Test
    {
        [Column("job_uuid", "TEXT PRIMARY KEY")]
        public string TestID { get; set; }

        [Column("name", "TEXT NOT NULL Unique")]
        public string Name { get; set; }

        [Column("age", "INTEGER")]
        public int Age { get; set; }

        [Column("create_time", "DATETIME")]
        public DateTime CreateTime { get; set; }
    }
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
                throw new InvalidOperationException($"The type {type.FullName} does not have a TableNameAttribute.");
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
                    if (columnInfo.Definition == "CLASS")
                    {
                        otherTableTypes.Add(GetEntityType(columnInfo.ColumnName));
                    }
                    else
                    {
                        otherTableTypes.Add(property.PropertyType);
                    }
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
                    return ("CLASS", columnName, isClassType);
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
        // 根据类名获取实体类型
        private Type GetEntityType(string entityFullName)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                return assembly.GetType(entityFullName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting entity type for {entityFullName}: {ex.Message}");
                return null;
            }
        }
        // 插入数据到表中
        public void InsertDataToTable(object entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "insert data cannot be null。");
            }

            Type type = entity.GetType();
            // 获取表名
            string tableName = GetTableName(type);

            // 判断表是否存在
            if (!IsTableExist(tableName))
            {
                CreateTable(type);
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new InvalidOperationException($"can not find table name for type {type.FullName} ");
            }

            // 构建插入语句的列名和值
            string columns = "";
            string values = "";
            PropertyInfo[] properties = type.GetProperties();
            bool isFirst = true;

            foreach (PropertyInfo property in properties)
            {
                ColumnAttribute columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute != null)
                {
                    if (!isFirst)
                    {
                        columns += ", ";
                        values += ", ";
                    }

                    columns += columnAttribute.ColumnName;
                    object propertyValue = property.GetValue(entity);
                    if (propertyValue is string)
                    {
                        values += $"'{propertyValue}'";
                    }
                    else if (propertyValue is DateTime)
                    {
                        values += $"'{((DateTime)propertyValue).ToString("yyyy-MM-dd HH:mm:ss")}'";
                    }
                    else
                    {
                        values += propertyValue?.ToString() ?? "NULL";
                    }

                    isFirst = false;
                }
            }

            if (string.IsNullOrEmpty(columns) || string.IsNullOrEmpty(values))
            {
                throw new InvalidOperationException($"can not find any column or value for type {type.FullName} ");
            }

            // 构建插入语句
            string insertQuery = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
            Console.WriteLine(insertQuery);

            // 执行插入语句
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        // 根据类型获取表名
        private string GetTableName(Type type)
        {
            TableNameAttribute tableNameAttribute = type.GetCustomAttribute<TableNameAttribute>();
            return tableNameAttribute?.TableName;
        }
        // 判断表是否存在
        public bool IsTableExist(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException(nameof(tableName), "table name cannot be null or empty。");
            }

            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = $"SELECT name FROM sqlite_master WHERE type='table' AND name = @TableName";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableName", tableName);
                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
        }
    }
    class Program
    {
        static void Main()
        {
            TableCreator tableCreator = new TableCreator("job_db.db");
            DataParese dataParese = new DataParese();
            var test = dataParese.SpiaderTest();
            foreach (var item in test)
            {
                foreach (var i in item)
                {
                    tableCreator.InsertDataToTable(i);
                }
            }
        }
    }
}
