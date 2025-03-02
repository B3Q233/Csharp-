using SpiderForJobInCore.CommonTool;
using SpiderForJobInCore.Model.DataAttribute;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;

namespace SpiderForJobInCore.Model.DAO
{
    // 该类用于创建数据库表和向表中插入数据
    public class TableCreator
    {
        // 数据库连接字符串
        private readonly string _connectionString;

        // 构造函数，初始化数据库连接字符串
        public TableCreator(string connectionString)
        {
            _connectionString = $"Data Source={connectionString};Version=3;";
        }

        // 泛型方法，根据传入的泛型类型创建对应的数据库表
        public void CreateTable<T>()
        {
            CreateTable(typeof(T));
        }

        // 根据传入的类型创建对应的数据库表
        public void CreateTable(Type type)
        {
            // 获取表名
            string tableName = GetTableName(type);
            // 检查是否存在表名，如果不存在则抛出异常
            if (string.IsNullOrEmpty(tableName))
            {
                throw new InvalidOperationException($"The type {type.FullName} does not have a TableNameAttribute.");
            }

            // 存储列定义的列表
            var columnDefinitions = new List<string>();
            // 存储外键定义的列表
            var foreignKeyDefinitions = new List<string>();
            // 存储需要创建的其他表类型的列表
            var otherTableTypes = new List<Type>();

            // 获取类型的所有属性
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                // 获取列的相关信息
                var columnInfo = GetColumnInfo(property, type, tableName);
                if (columnInfo.IsClassType)
                {
                    if (columnInfo.Definition == "CLASS")
                    {
                        // 根据类名获取实体类型
                        var retType = GetEntityType(columnInfo.ColumnName);
                        // 将该类型添加到需要创建的其他表类型列表中
                        otherTableTypes.Add(retType);
                        Console.WriteLine(retType.Name);
                        // 添加列定义
                        columnDefinitions.Add(ModelCommonTool.ConvertCamelCaseToSnakeCase($"{retType.Name}") + " INTEGER");
                    }
                    else
                    {
                        // 将属性类型添加到需要创建的其他表类型列表中
                        otherTableTypes.Add(property.PropertyType);
                    }
                }
                else
                {
                    // 添加列定义
                    columnDefinitions.Add(columnInfo.Definition);
                    // 获取外键定义
                    var foreignKeyDefinition = GetForeignKeyDefinition(property, columnInfo.ColumnName);
                    if (!string.IsNullOrEmpty(foreignKeyDefinition))
                    {
                        // 如果外键定义不为空，则添加到外键定义列表中
                        foreignKeyDefinitions.Add(foreignKeyDefinition);
                    }
                }
            }

            // 检查是否存在列定义，如果不存在则抛出异常
            if (!columnDefinitions.Any())
            {
                throw new InvalidOperationException($"No columns found for type {type.Name}.");
            }

            // 将外键定义添加到列定义列表中
            columnDefinitions.AddRange(foreignKeyDefinitions);
            // 将列定义用逗号连接成字符串
            string columnsDefinition = string.Join(", ", columnDefinitions);
            // 构建创建表的 SQL 语句
            string createTableQuery = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnsDefinition});";

            // 执行创建表的 SQL 语句
            ExecuteQuery(createTableQuery);

            // 递归创建其他需要创建的表
            foreach (var otherTableType in otherTableTypes)
            {
                CreateTable(otherTableType);
            }
        }

        // 获取列的相关信息，包括定义、列名和是否为类类型
        private (string Definition, string ColumnName, bool IsClassType) GetColumnInfo(PropertyInfo property, Type type, string tableName)
        {
            // 默认列类型为 TEXT
            const string defaultColumnType = "TEXT";
            // 获取列属性
            var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            string columnName;
            string columnType;
            bool isClassType = false;

            if (columnAttribute != null)
            {
                // 如果存在列属性，则获取列名和列类型
                columnName = columnAttribute.ColumnName;
                columnType = columnAttribute.ColumnType;
                if (columnType == "CLASS")
                {
                    // 检查属性类型是否与类本身相同，如果相同则抛出异常
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
                // 如果不存在列属性，则将属性名转换为蛇形命名作为列名，使用默认列类型
                columnName = ModelCommonTool.ConvertCamelCaseToSnakeCase(property.Name);
                columnType = defaultColumnType;
            }

            return ($"{columnName} {columnType}", columnName, isClassType);
        }

        // 获取外键定义
        private string GetForeignKeyDefinition(PropertyInfo property, string columnName)
        {
            // 获取外键属性
            var foreignKeyAttribute = property.GetCustomAttribute<ForeignKeyAttribute>();
            // 如果存在外键属性，则返回外键定义，否则返回空字符串
            return foreignKeyAttribute != null
                ? $"FOREIGN KEY ({columnName}) REFERENCES {foreignKeyAttribute.ReferencedTableName}({foreignKeyAttribute.ReferencedColumnName})"
                : string.Empty;
        }

        // 根据类名获取实体类型
        private Type GetEntityType(string entityFullName)
        {
            try
            {
                // 获取当前执行的程序集
                Assembly assembly = Assembly.GetExecutingAssembly();
                // 根据类名从程序集中获取类型
                return assembly.GetType(entityFullName);
            }
            catch (Exception ex)
            {
                // 捕获异常并输出错误信息
                Console.WriteLine($"Error getting entity type for {entityFullName}: {ex.Message}");
                return null;
            }
        }

        // 向表中插入数据
        public void InsertDataToTable(object entity)
        {
            // 检查插入的数据是否为空，如果为空则抛出异常
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "insert data cannot be null.");
            }

            // 获取实体的类型
            Type type = entity.GetType();
            // 获取表名
            string tableName = GetTableName(type);

            // 检查是否存在表名，如果不存在则抛出异常
            if (string.IsNullOrEmpty(tableName))
            {
                throw new InvalidOperationException($"can not find table name for type {type.FullName} ");
            }

            // 检查表是否存在，如果不存在则创建表
            if (!IsTableExist(tableName))
            {
                CreateTable(type);
            }

            // 获取插入语句的列名和值
            var (columns, values) = GetInsertColumnsAndValues(entity);

            // 检查列名和值是否为空，如果为空则抛出异常
            if (string.IsNullOrEmpty(columns) || string.IsNullOrEmpty(values))
            {
                throw new InvalidOperationException($"can not find any column or value for type {type.FullName} ");
            }

            // 构建插入数据的 SQL 语句
            string insertQuery = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
            // 执行插入数据的 SQL 语句
            ExecuteQuery(insertQuery);
        }

        // 获取插入语句的列名和值
        private (string Columns, string Values) GetInsertColumnsAndValues(object entity)
        {
            // 获取实体的类型
            Type type = entity.GetType();
            // 存储列名的列表
            var columnsList = new List<string>();
            // 存储列值的列表
            var valuesList = new List<string>();

            // 获取类型的所有属性
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                // 获取列属性
                ColumnAttribute columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                // 如果存在列属性，则使用列属性中的列名，否则将属性名转换为蛇形命名作为列名
                string columnName = columnAttribute != null
                    ? columnAttribute.ColumnName
                    : ModelCommonTool.ConvertCamelCaseToSnakeCase(property.Name);

                // 获取属性的值
                object propertyValue = property.GetValue(entity);
                // 获取列值
                string columnContent = GetColumnValue(propertyValue);

                // 将列名添加到列名列表中
                columnsList.Add(columnName);
                // 将列值添加到列值列表中
                valuesList.Add(columnContent);
            }

            // 将列名列表用逗号连接成字符串
            string columns = string.Join(", ", columnsList);
            // 将列值列表用逗号连接成字符串
            string values = string.Join(", ", valuesList);

            return (columns, values);
        }

        // 获取列值
        private string GetColumnValue(object propertyValue)
        {
            if (propertyValue == null)
            {
                // 如果属性值为空，则返回 "NULL"
                return "NULL";
            }

            if (propertyValue is string strValue)
            {
                // 如果属性值为字符串，替换单引号并添加引号
                strValue = strValue.Replace("'", "''");
                return $"'{strValue}'";
            }

            if (propertyValue is DateTime dateTimeValue)
            {
                // 如果属性值为日期时间类型，将其格式化为字符串并添加引号
                return $"'{dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss")}'";
            }

            // 其他类型，将其转换为字符串并添加引号
            return $"'{propertyValue}'";
        }

        // 根据类型获取表名
        private string GetTableName(Type type)
        {
            // 获取表名属性
            TableNameAttribute tableNameAttribute = type.GetCustomAttribute<TableNameAttribute>();
            // 返回表名，如果不存在则返回 null
            return tableNameAttribute?.TableName;
        }

        // 检查表是否存在
        public bool IsTableExist(string tableName)
        {
            // 检查表名是否为空，如果为空则抛出异常
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException(nameof(tableName), "table name cannot be null or empty.");
            }

            // 构建检查表是否存在的 SQL 语句
            string query = $"SELECT name FROM sqlite_master WHERE type='table' AND name = @TableName";
            using (var connection = new SQLiteConnection(_connectionString))
            using (var command = new SQLiteCommand(query, connection))
            {
                // 打开数据库连接
                connection.Open();
                // 添加参数
                command.Parameters.AddWithValue("@TableName", tableName);
                // 执行查询并获取结果
                object result = command.ExecuteScalar();
                // 返回结果是否不为空
                return result != null;
            }
        }

        // 执行 SQL 插入查询
        private void ExecuteQuery(string query)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            using (var command = new SQLiteCommand(query, connection))
            {
                try
                {
                    // 打开数据库连接
                    connection.Open();
                    // 执行 SQL 语句
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // 捕获异常并抛出包含查询语句的异常
                    throw new Exception($"Error executing query: {query}", ex);
                }
            }
        }
    }
}
