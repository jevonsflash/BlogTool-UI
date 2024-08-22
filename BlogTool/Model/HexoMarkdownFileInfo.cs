using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using BlogTool.Core.Markdown;

namespace BlogTool.Model
{

    public class HexoMarkdownFileInfo : IMarkdown
    {
        public List<string> Categories { get; set; } = new List<string>();

        public string Title
        {
            get;
            set;
        }

        public string Keywords
        {
            get;
            set;
        }

        public string Link
        {
            get;
            set;
        }

        public DateTime? DateCreated
        {
            get;
            set;
        }

      
        public string FilePath
        {
            get;
            set;
        }
        public string Description {get; set; }  
        public string Content { get; set; }  
    }

}
