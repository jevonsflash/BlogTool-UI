using BlogTool.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace BlogTool.Core.Markdown.Implements
{
    public class TextMarkdownProvider : MarkdownProvider
    {
        public override ICollection<IMarkdown> GetMarkdowns(GetMarkdownOption option, params object[] objects)
        {
            var markdowns = new List<IMarkdown>();
            var p = objects[0] as dynamic;
            markdowns.Add(new PostInfo()
            {
                Categories = new List<string>()
                {

                },
                Title = p.Title,
                Content = p.Content,
                DateCreated = DateTime.Now,
            });


            return markdowns;
        }
    }
}
