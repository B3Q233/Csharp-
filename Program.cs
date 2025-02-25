using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System;
using System.Threading.Tasks;
using static SpiderForJobInCore.DataParese;

namespace PuppeteerSharpXhrExample
{
    class Program
    {
        static async Task Main()
        {
            DataParse();
            return;
            int MAX_PAGE = 100;
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
                        await JsonParser.ParseJsonDataAsync(json.ToString(), Environment.CurrentDirectory + $"/data/{cnt}_page.json");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"无法将响应内容解析为 JSON: {ex.Message}");
                    }
                }
            };

            await page.GoToAsync("https://we.51job.com/pc/search?keyword=c%23&searchType=2&sortType=0&metro=&pageNum=2");

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