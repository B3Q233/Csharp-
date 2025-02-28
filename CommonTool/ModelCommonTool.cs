using System;
using System.Text;

namespace SpiderForJobInCore.CommonTool
{
    class ModelCommonTool
    {

        // 将字符串转换成下划线命名法（Snake Case）
        public static string? ConvertCamelCaseToSnakeCase(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            // 去除字符串首尾的空格，并将连续的多个空格替换为一个空格
            string trimmedStr = System.Text.RegularExpressions.Regex.Replace(str.Trim(), @"\s+", " ");
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < trimmedStr.Length; i++)
            {
                if (i > 0 && char.IsUpper(trimmedStr[i]))
                {
                    result.Append('_');
                }
                result.Append(char.ToLower(trimmedStr[i]));
            }
            return result.ToString();
        }

        // 获取UUID
        public static string GetUUID()
        {
            Guid uuid = Guid.NewGuid();
            return uuid.ToString();
        }
    }

}
