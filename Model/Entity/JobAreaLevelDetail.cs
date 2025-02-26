using System.ComponentModel.DataAnnotations.Schema;

namespace SpiderForJobInCore.Model.Entity
{
    // 该类用于存储工作地点的详细区域信息，包含省、市、区等相关信息
    [Table("job_area_level_detail_db")]
    public class JobAreaLevelDetail
    {
        // 省份的编码，可能是一个用于标识省份的特定代码
        public string provinceCode { get; set; }
        // 省份的名称，例如“上海”“北京”等
        public string provinceString { get; set; }
        // 城市的编码，用于唯一标识某个城市
        public string cityCode { get; set; }
        // 城市的名称，如“上海”“广州”等
        public string cityString { get; set; }
        // 区/县的名称，例如“闵行区”“朝阳区”等
        public string districtString { get; set; }
        // 地标信息，可能是具体的地点或标志性建筑
        public string landMarkString { get; set; }
    }
}
