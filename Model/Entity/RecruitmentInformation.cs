using SpiderForJobInCore.Model.DataAttribute;
using System.Collections.Generic;

namespace SpiderForJobInCore.Model.Entity
{
    // 招聘信息类，包含了一个工作职位的完整信息
    [TableName("recruitment_information_db")]
    public class RecruitmentInformation
    {
        // 工作职位的名称
        public string JobName { get; set; }

        // 工作地点的详细区域信息
        public JobAreaLevelDetail JobAreaLevelDetail { get; set; }

        // 提供的薪资范围
        [Column("NONE", "NONE")]
        public string ProvideSalaryString { get; set; }
        // 职位发布的日期
        public string IssueDateString { get; set; }

        // 职位确认的日期
        [Column("NONE", "NONE")]
        public string ConfirmDateString { get; set; }

        // 工作经验要求的代码或标识
        public string WorkYear { get; set; }

        // 工作经验要求的文字描述
        public string WorkYearString { get; set; }

        // 学历要求的文字描述
        public string DegreeString { get; set; }

        // 行业类型 1 的代码或标识
        [Column("NONE", "NONE")]
        public string IndustryType1 { get; set; }

        // 行业类型 2 的代码或标识
        [Column("NONE", "NONE")]
        public string IndustryType2 { get; set; }

        // 行业类型 1 的文字描述
        [Column("industry_type", "TEXT")]
        public string IndustryType1Str { get; set; }

        // 行业类型 2 的文字描述
        [Column("NONE", "NONE")]
        public string IndustryType2Str { get; set; }

        // 公司的简称
        [Column("NONE", "NONE")]
        public string CompanyName { get; set; }

        // 公司的全称
        public string FullCompanyName { get; set; }

        // 公司类型的文字描述
        public string CompanyTypeString { get; set; }

        // 公司规模的文字描述
        public string CompanySizeString { get; set; }

        // 公司规模的代码或标识
        [Column("NONE", "NONE")]
        public string CompanySizeCode { get; set; }

        // 公司所属行业类型 1 的文字描述
        [Column("company_industry_type", "TEXT")]
        public string CompanyIndustryType1Str { get; set; }

        // 公司所属行业类型 2 的文字描述
        [Column("NONE", "NONE")]
        public string CompanyIndustryType2Str { get; set; }

        // 职位信息更新的日期和时间
        public string UpdateDateTime { get; set; }

        // 工作地点的经度
        [Column("NONE", "NONE")]
        public string Lon { get; set; }

        // 工作地点的纬度
        [Column("NONE", "NONE")]
        public string Lat { get; set; }

        // 职位详情页面的链接
        public string JobHref { get; set; }

        // 职位的详细描述
        [Column("NONE", "NONE")]
        public string JobDescribe { get; set; }

        // 工作的期限类型
        public string TermStr { get; set; }

        // 工作标签的列表
        [Column("NONE", "FORIGN")]
        public List<JobTagsListItem> JobTagsList { get; set; }

        // 职位薪资的最高值
        [Column("job_salary_max", "REAL")]
        public string JobSalaryMax { get; set; }

        // 职位薪资的最低值
        [Column("job_salary_min", "REAL")]
        public string JobSalaryMin { get; set; }
    }
}

