using System.ComponentModel.DataAnnotations.Schema;

namespace SpiderForJobInCore.Model.Entity
{
    // 该类用于存储工作标签的相关信息，每个标签有一个名称
    [Table("job_tags_list_db")]
    public class JobTagsListItem
    {
        // 工作标签的名称，例如“5年及以上”“本科”等
        public string jobTagName { get; set; }
    }
}
