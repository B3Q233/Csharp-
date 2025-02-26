using System;

namespace SpiderForJobInCore.Model.DataAttribute
{
    // 自定义特性 主要用于利用反射构造字符串写入SQlite数据库时，根据自定义特性来构造表名和字段名以及其类型

    // 用于标记类对应的表名
    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute
    {
        public string TableName { get; }
        public TableNameAttribute(string tableName)
        {
            TableName = tableName;
        }
    }

    // 用于标记属性对应的字段名和字段类型
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; }
        public string ColumnType { get; }

        public ColumnAttribute(string columnName, string columnType)
        {
            ColumnName = columnName;
            ColumnType = columnType;
        }
    }
}
