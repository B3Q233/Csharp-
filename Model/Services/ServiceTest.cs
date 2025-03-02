using SpiderForJobInCore.Model.DataAttribute;
using System;
using System.Threading.Tasks;

namespace SpiderForJobInCore.Model.Services
{
    // 定义一个名为 "test" 的数据表对应的实体类
    [TableName("test")]
    public class Test
    {
        // 定义一个名为 "job_uuid" 的列，类型为 TEXT 且为主键
        [Column("job_uuid", "TEXT PRIMARY KEY")]
        public string TestID { get; set; }

        // 定义一个名为 "name" 的列，类型为 TEXT，非空且唯一
        [Column("name", "TEXT NOT NULL Unique")]
        public string Name { get; set; }

        // 定义一个名为 "age" 的列，类型为 INTEGER
        [Column("age", "INTEGER")]
        public int Age { get; set; }

        // 定义一个名为 "create_time" 的列，类型为 DATETIME
        [Column("create_time", "DATETIME")]
        public DateTime CreateTime { get; set; }
        public static async Task Main()
        {
        }
    }
}
