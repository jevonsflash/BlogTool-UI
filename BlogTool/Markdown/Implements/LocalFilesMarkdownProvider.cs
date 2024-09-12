using BlogTool.Core.Aigc.DashScope;
using BlogTool.Core.Aigc.DashScope.TextGeneration;
using BlogTool.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using BlogTool.Core.Markdown;

namespace BlogTool.Markdown.Implements
{
    public class LocalFilesMarkdownProvider : MarkdownProvider
    {
        private IAigcClient aigcClientClient;

        public const string DescriptionGenerationMsg = "你是一个摘要生成工具，你需要解释我发送给你的内容，不要换行，不要超过200字，只包含纯文本，不要用Markdown格式，只需要介绍文章的内容，不需要提出建议和缺少的东西。请用中文回答，文章内容为：";
        public const string TitleGenerationMsg = "生成标题，只包含纯文本，不要用Markdown格式，请用中文回答";
        public const string TagsGenerationMsg = "生成5个标签，每个标签不要超过4个字符，用/分割，只包含纯文本，不要用Markdown格式，请用中文回答";
        public ChatParameters chatParameters = new ChatParameters() { Seed=1234, MaxTokens=1500, Temperature=(float?)0.5, TopP=(float?)0.4, RepetitionPenalty=1 };

        public override ICollection<IMarkdown> GetMarkdowns(GetMarkdownOption option, params object[] objects)
        {
            if (!string.IsNullOrEmpty(option.AigcOption.Provider)&&!string.IsNullOrEmpty(option.AigcOption.ApiKey))
            {
                switch (option.AigcOption.Provider)
                {
                    case DashScopeClient.Name:
                        aigcClientClient = new DashScopeClient(option.AigcOption.ApiKey);
                        break;

                    default:
                        break;
                }
            }
            var markdowns = new List<IMarkdown>();
            var filePaths = objects as string[];

            foreach (var filePath in filePaths)
            {
                try
                {
                    var content = File.ReadAllText(filePath);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    Console.WriteLine($"Found Markdown file: {Path.GetFileName(filePath)}");
                    var description = "";
                    var tags = new List<string>();
                    var title = "";
                    try
                    {
                        Console.WriteLine($"Generating description with AI ...");

                        var target = option.AigcOption.Target.Split(',');
                        if (aigcClientClient!=default && !string.IsNullOrEmpty(content) && target.Contains("Description"))
                        {
                            var _textGenerationClient = aigcClientClient.TextGeneration;
                            var descResult = _textGenerationClient.Chat("qwen-turbo", [
    new ChatMessage("user", $"{DescriptionGenerationMsg}{content}"),
                ], chatParameters).Result;
                            var aiDesc = descResult.Output.Text;

                            var titleResult = _textGenerationClient.Chat("qwen-turbo", [
    new ChatMessage("user", $"{DescriptionGenerationMsg}{content}"),
    new ChatMessage("assistant", $"{aiDesc}"),
    new ChatMessage("user", $"{TitleGenerationMsg}"),
                ], chatParameters).Result;

                            var aiTitle = titleResult.Output.Text;

                            var tagsResult = _textGenerationClient.Chat("qwen-turbo", [
    new ChatMessage("user", $"{DescriptionGenerationMsg}{content}"),
    new ChatMessage("assistant", $"{aiDesc}"),
    new ChatMessage("user", $"{TitleGenerationMsg}"),
    new ChatMessage("assistant", $"{aiTitle}"),
    new ChatMessage("user", $"{TagsGenerationMsg}"),
                ], chatParameters).Result;

                            var aiTags = tagsResult.Output.Text;

                            description=aiDesc;
                            title=string.IsNullOrEmpty(fileNameWithoutExtension) ? aiTitle : fileNameWithoutExtension;
                            tags=aiTags.Split('/').ToList();
                        }



                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error generating description with AI, post {fileNameWithoutExtension}: {ex.Message}");
                    }

                    markdowns.Add(new PostInfo()
                    {
                        Categories = new List<string>()
                        {

                        },
                        Title = title,
                        Keywords=string.Join(',', tags),
                        Content = content,
                        DateCreated = DateTime.Now,
                        Description= description,
                    });

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file {filePath}: {ex.Message}");
                }
            }




            return markdowns;
        }
    }
}
