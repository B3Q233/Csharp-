﻿using SpiderForJobInCore.Model.DataAttribute;

namespace SpiderForJobInCore.Model.Entity
{
    // 该类用于存储工作标签的相关信息，每个标签有一个名称
    [TableName("job_tags_list_item")]
    public class JobTagsListItem
    {
        [Column("job_tags_list_ID", "TEXT PRIMARY KEY")]
        public string JobTagsListId { get; set; }
        // 工作标签的名称，例如“5年及以上”“本科”等
        [Column("job_tag_name", "TEXT UNIQUE")]
        public string JobTagName { get; set; }
    }
}
