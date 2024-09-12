using BlogTool.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace BlogTool.Helper
{
    internal class YamlHelper
    {
        private static IDeserializer deserializer = new DeserializerBuilder()
.WithNamingConvention(new CamelCaseNamingConvention()) // 根据需要选择命名约定  
.Build();

        private static ISerializer serializer = new SerializerBuilder()
    .WithNamingConvention(new CamelCaseNamingConvention()) // 根据需要选择命名约定  
    .Build();
        public static object ReadHexoPostMetadata(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found: " + filePath);
                return null;
            }

            string content = File.ReadAllText(filePath);

            // 查找 YAML 头部信息的开始和结束位置  
            int start = content.IndexOf("---");
            int end = content.LastIndexOf("---") + 3;

            return ReadHexoPostMetadata(start, end, content);
        }

        public static object ReadHexoPostMetadata(int start, int end, string content)
        {

            if (start == -1 || end == -1 || start >= end)
            {
                Console.WriteLine("No YAML front matter found in the file.");
                return null;
            }

            // 提取 YAML 头部信息  
            string yamlContent = content[start..end];
            string pattern = @"^---\s*$";
            yamlContent = Regex.Replace(yamlContent, pattern, "", RegexOptions.Multiline).Trim();
            yamlContent=$"---\n{yamlContent}";
            // 使用 YamlDotNet 解析 YAML 内容  


            try
            {
                return deserializer.Deserialize(yamlContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deserializing YAML: " + ex.Message);
                return null;
            }
        }
        public static string WriteHexoPostMetadata(int start, int end, string content, object hexoPostMetadata)
        {
            try
            {
                string before = content.Substring(0, start);
                string replaced = $"---\n{serializer.Serialize(hexoPostMetadata)}\n---\n";
                string after = content.Substring(end);
                content = string.Concat(before, replaced, after);
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error serializing YAML: " + ex.Message);
                return null;

            }

        }

    }
}
