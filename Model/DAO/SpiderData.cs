using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using SpiderForJobInCore.Model.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiderForJobInCore.Model.DAO
{
    public class SpiderData
    {
        private TableCreator _tableCreator;
        private string _dBName;
        private Dictionary<string, int> _allTagDict;
        private Dictionary<string, int> _alljobDict;
        SpiderData(string dbName)
        {
            _dBName = dbName;
            _tableCreator = new TableCreator(_dBName);
        }
        private void InitDict()
        {

        }
        public void InsertToDB(ref List<List<RecruitmentInformation>> allItems, ref List<List<WholeRecruitmentInformation>> wholeRecruitmentInformations)
        {
            // 获取 allItems 的行数
            int row = allItems.Count;
            // 获取 allItems 的列数
            int colum = allItems[0].Count;
            // 职位标签编号计数器
            int cnt = 0;
            // 插入的职位标签数量计数器
            int count = 0;
            // 存储职位标签及其编号的字典
            var jobTagDict = new Dictionary<string, int>();
            // 存储职位 ID 及其编号的字典
            var jobIdDict = new Dictionary<string, int>();

            // 遍历 allItems 二维列表
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < colum; j++)
                {
                    // 获取当前招聘信息
                    var jobItems = allItems[i][j];
                    // 获取当前招聘信息的区域详细信息
                    var jobArealLevelDtail = wholeRecruitmentInformations[i][j].JobAreaLevelDetail;
                    // 获取当前招聘信息的职位标签列表
                    var jobTagsList = wholeRecruitmentInformations[i][j].JobTagsList;

                    // 检查职位 ID 是否已经存在于 jobIdDict 中，如果存在则跳过
                    if (jobIdDict.ContainsKey(jobItems.JobId))
                    {
                        continue;
                    }

                    // 将职位 ID 添加到 jobIdDict 中
                    jobIdDict[jobItems.JobId] = 1;

                    // 存储职位标签编号的字符串
                    string jobTagCode = "";
                    foreach (var tag in jobTagsList)
                    {
                        // 检查职位标签是否已经存在于 jobTagDict 中，如果不存在则添加
                        if (!jobTagDict.ContainsKey(tag.JobTagName))
                        {
                            jobTagDict[tag.JobTagName] = cnt++;
                            tag.JobTagsListId = (cnt - 1).ToString();
                            count++;
                            // 向表中插入职位标签数据
                            _tableCreator.InsertDataToTable(tag);
                        }

                        // 获取职位标签编号并添加到 jobTagCode 中
                        string tagCode = jobTagDict[tag.JobTagName].ToString();
                        jobTagCode += tagCode + " ";
                    }

                    // 输出职位 ID
                    Console.WriteLine(jobItems.JobId);

                    // 设置招聘信息的城市、省份、职位标签编号和地区信息
                    jobItems.City = jobArealLevelDtail?.ProvinceString;
                    jobItems.Province = jobArealLevelDtail?.CityString;
                    jobItems.JobTagCode = jobTagCode;
                    jobItems.District = jobArealLevelDtail?.DistrictString;

                    // 向表中插入招聘信息数据
                    _tableCreator.InsertDataToTable(jobItems);
                }
                // 换行
                Console.WriteLine();
            }
            // 输出职位标签字典的数量
            Console.WriteLine(jobTagDict.Count);
            // 输出插入的职位标签数量
            Console.WriteLine(count);
        }
        public static async Task SpiderTheData()
        {
            // 存储招聘信息的二维列表
            List<List<RecruitmentInformation>> allItems = new List<List<RecruitmentInformation>>();
            // 存储完整招聘信息的二维列表
            List<List<WholeRecruitmentInformation>> wholeRecruitmentInformations = new List<List<WholeRecruitmentInformation>>();
            // 存储职位标签及其编号的字典
            var jobTagDict = new Dictionary<string, int>();
            // 存储职位 ID 及其编号的字典
            var jobIdDict = new Dictionary<string, int>();
            int MAX_PAGE = 2;
            int cnt = 1;

            await new BrowserFetcher().DownloadAsync();
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = false });
            using var page = await browser.NewPageAsync();
            await page.SetRequestInterceptionAsync(true);

            page.Request += async (sender, e) =>
            {
                await e.Request.ContinueAsync();
            };

            page.Response += async (sender, e) =>
            {
                if (e.Response.Request.ResourceType == ResourceType.Xhr && e.Response.Url.Contains("https://we.51job.com/api/job/search-pc?api_key="))
                {
                    try
                    {
                        var responseText = await e.Response.TextAsync();
                        var json = JToken.Parse(responseText);
                        var pageConten = await DataParse.ParseJsonDataAsync(json.ToString(), Environment.CurrentDirectory + $"/data/{cnt}_page.json");
                        // 调用 DataParese 的 SpiaderTest 方法进行数据解析
                        //Console.WriteLine(pageConten);
                        DataParse.ContentParse(pageConten, ref allItems, ref wholeRecruitmentInformations);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"无法将响应内容解析为 JSON: {ex.Message}");
                    }
                }
            };

            await page.GoToAsync("https://we.51job.com/pc/search?keyword=c++&searchType=2&sortType=0&metro=&pageNum=2");

            while (cnt < MAX_PAGE)
            {
                Console.WriteLine($"正在爬取第 {cnt} 页数据...");
                int nextPage = cnt + 1;
                await page.WaitForNetworkIdleAsync();

                try
                {
                    IElementHandle[] li = await page.QuerySelectorAllAsync("li[class=\"number\"]");
                    bool pageClicked = false;
                    foreach (var item in li)
                    {
                        string textContent = await item.EvaluateFunctionAsync<string>("(el) => el.textContent");
                        if (textContent.Equals($"{nextPage}"))
                        {
                            await item.ClickAsync();
                            pageClicked = true;
                            break;
                        }
                    }

                    if (!pageClicked)
                    {
                        Console.WriteLine($"未找到第 {nextPage} 页的元素，退出循环。");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"点击第 {nextPage} 页元素时出错: {ex.Message}");
                    break;
                }

                cnt++;
                // 等待特定元素加载完成，确保页面完全加载
                await page.WaitForSelectorAsync("li[class=\"number\"]", new WaitForSelectorOptions { Timeout = 5000 });
                await Task.Delay(3000);
            }

            // 显式关闭浏览器
            await browser.CloseAsync();
            Console.ReadLine();
        }
    }
}