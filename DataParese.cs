using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpiderForJobInCore
{
    internal class DataParese
    {
        public static void DataParse()
        {
            string folderPath = Environment.CurrentDirectory + @"\data\";
            string[] allFiles = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            List<List<Root>> allItems = new List<List<Root>>();
            foreach (string file in allFiles)
            {
                string content = File.ReadAllText(file);
                var items = JsonSerializer.Deserialize<List<Root>>(content);
                allItems.Add(items);
            }
            int cnt = 0;
            int cntLeserThanOneYear = 0;
            foreach (List<Root> items in allItems)
            {
                foreach (Root item in items)
                {
                    //Console.WriteLine(item.workYearString);
                    cnt++;
                    int age = 0;
                    foreach (char c in item.workYear)
                    {
                        age = age * 10 + (c - '0');
                    }
                    if (item.workYearString[0] == '无')
                    {
                        Console.WriteLine(item.jobHref);
                        cntLeserThanOneYear++;
                    }
                }
            }
            Console.WriteLine(cntLeserThanOneYear);
        }

        public class JsonParser
        {
            public async static Task<bool> ParseJsonDataAsync(string jsonString, string saveFilePath)
            {
                // 输入验证
                if (string.IsNullOrEmpty(jsonString))
                {
                    Console.WriteLine("输入的 JSON 字符串为空。");
                    return false;
                }
                try
                {
                    // 提取 "items" 数组部分
                    int startIndex = jsonString.IndexOf("\"items\": [");
                    if (startIndex == -1)
                    {
                        Console.WriteLine("未找到 \"items\" 数组。");
                        return false;
                    }
                    startIndex += "\"items\": [".Length;

                    int endIndex = jsonString.LastIndexOf("],");
                    if (endIndex == -1)
                    {
                        Console.WriteLine("未找到 \"items\" 数组的结束标记。");
                        return false;
                    }

                    string itemsJson = "[" + jsonString.Substring(startIndex, endIndex - startIndex) + "]";

                    // 反序列化 JSON 数据
                    var items = JsonSerializer.Deserialize<List<Root>>(itemsJson);
                    if (items == null)
                    {
                        Console.WriteLine("无法将 JSON 数据反序列化为 List<Root>。");
                        return false;
                    }

                    await File.WriteAllTextAsync(saveFilePath, itemsJson.Trim());
                    Console.WriteLine($"JSON 数据已保存到 {saveFilePath}");

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理 JSON 数据时发生错误: {ex.Message}");
                    return false;
                }
            }
        }
        // 该类用于存储工作地点的详细区域信息，包含省、市、区等相关信息
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

        // 该类用于存储工作标签的相关信息，每个标签有一个名称
        public class JobTagsListItem
        {
            // 工作标签的名称，例如“5年及以上”“本科”等
            public string jobTagName { get; set; }
        }

        // 该类是根类，包含了一个工作职位的完整信息
        public class Root
        {
            // 工作职位的名称，如“net C# 软件开发工程师”
            public string jobName { get; set; }
            // 工作地点的详细区域信息，使用 JobAreaLevelDetail 类来存储
            public JobAreaLevelDetail jobAreaLevelDetail { get; set; }
            // 提供的薪资范围，以字符串形式表示，如“1.5 - 2万”
            public string provideSalaryString { get; set; }
            // 职位发布的日期，以字符串形式存储
            public string issueDateString { get; set; }
            // 职位确认的日期，以字符串形式存储
            public string confirmDateString { get; set; }
            // 工作经验要求的代码或标识
            public string workYear { get; set; }
            // 工作经验要求的文字描述，如“5年及以上”
            public string workYearString { get; set; }
            // 学历要求的文字描述，如“本科”“硕士”等
            public string degreeString { get; set; }
            // 行业类型 1 的代码或标识
            public string industryType1 { get; set; }
            // 行业类型 2 的代码或标识
            public string industryType2 { get; set; }
            // 行业类型 1 的文字描述，如“仪器仪表/工业自动化”
            public string industryType1Str { get; set; }
            // 行业类型 2 的文字描述，如“仪器仪表/工业自动化”
            public string industryType2Str { get; set; }
            // 公司的简称，如“上海朴维自控科技”
            public string companyName { get; set; }
            // 公司的全称，如“上海朴维自控科技有限公司”
            public string fullCompanyName { get; set; }
            // 公司类型的文字描述，如“民营”“国企”等
            public string companyTypeString { get; set; }
            // 公司规模的文字描述，如“50 - 150人”
            public string companySizeString { get; set; }
            // 公司规模的代码或标识
            public string companySizeCode { get; set; }
            // 公司所属行业类型 1 的文字描述，如“仪器仪表/工业自动化”
            public string companyIndustryType1Str { get; set; }
            // 公司所属行业类型 2 的文字描述，如“仪器仪表/工业自动化”
            public string companyIndustryType2Str { get; set; }
            // 职位信息更新的日期和时间，以字符串形式存储
            public string updateDateTime { get; set; }
            // 工作地点的经度，以字符串形式存储
            public string lon { get; set; }
            // 工作地点的纬度，以字符串形式存储
            public string lat { get; set; }
            // 职位详情页面的链接
            public string jobHref { get; set; }
            // 职位的详细描述，包括岗位职责和岗位要求等信息
            public string jobDescribe { get; set; }
            // 工作的期限类型，如“全职”“兼职”等
            public string termStr { get; set; }
            // 工作标签的列表，每个标签是一个 JobTagsListItem 对象
            public List<JobTagsListItem> jobTagsList { get; set; }
            // 职位薪资的最高值，以字符串形式存储
            public string jobSalaryMax { get; set; }
            // 职位薪资的最低值，以字符串形式存储
            public string jobSalaryMin { get; set; }
        }
    }
}