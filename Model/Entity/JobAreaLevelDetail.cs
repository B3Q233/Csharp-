using SpiderForJobInCore.Model.DataAttribute;

namespace SpiderForJobInCore.Model.Entity
{
    // 该类用于存储工作地点的详细区域信息，包含省、市、区等相关信息
    [TableName("job_area_level_detail")]
    public class JobAreaLevelDetail
    {
        [Column("job_uuid", "TEXT PRIMARY KEY")]
        public string JobAreaLevelDetailId { get; set; }
        // 省份的编码，可能是一个用于标识省份的特定代码
        public string ProvinceCode { get; set; }
        // 省份的名称，例如“上海”“北京”等
        public string ProvinceString { get; set; }
        // 城市的编码，用于唯一标识某个城市
        public string CityCode { get; set; }
        // 城市的名称，如“上海”“广州”等
        public string CityString { get; set; }
        // 区/县的名称，例如“闵行区”“朝阳区”等
        public string DistrictString { get; set; }

    }
}
