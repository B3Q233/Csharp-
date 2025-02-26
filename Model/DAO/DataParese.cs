using SpiderForJobInCore.Model.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpiderForJobInCore.Model.DAO
{
    internal class DataParese
    {
        // 将爬取的文件存入到本地json文件中，进行数据检查
        public void SpiaderTest()
        {
            string folderPath = Environment.CurrentDirectory + @"\data\";
            string[] allFiles = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            List<List<RecruitmentInformation>> allItems = new List<List<RecruitmentInformation>>();
            foreach (string file in allFiles)
            {
                string content = File.ReadAllText(file);
                var items = JsonSerializer.Deserialize<List<RecruitmentInformation>>(content);
                allItems.Add(items);
            }
            int cnt = 0;
            int cntLeserThanOneYear = 0;
            foreach (List<RecruitmentInformation> items in allItems)
            {
                foreach (RecruitmentInformation item in items)
                {
                    //Console.WriteLine(item.workYearString);
                    cnt++;
                    int age = 0;
                    foreach (char c in item.WorkYear)
                    {
                        age = age * 10 + (c - '0');
                    }
                    if (item.WorkYearString[0] == '1')
                    {
                        Console.WriteLine(item.JobAreaLevelDetail.provinceString);
                        //Console.WriteLine(item.jobDescribe);
                        cntLeserThanOneYear++;
                    }
                }
            }
            Console.WriteLine(cntLeserThanOneYear);
        }
        public static void DataParse()
        {

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
                    var items = JsonSerializer.Deserialize<List<RecruitmentInformation>>(itemsJson);
                    if (items == null)
                    {
                        Console.WriteLine("无法将 JSON 数据反序列化为 List<Recruitmentinformation>。");
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
    }
}