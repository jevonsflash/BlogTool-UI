using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using BlogTool.Core.Markdown;
using System.ComponentModel;

namespace BlogTool.Model
{

    public class HexoMarkdownFileInfo : IMarkdown
    {
        public List<string> Categories { get; set; } = new List<string>();

        [DisplayName("标题")]
        public string Title
        {
            get;
            set;
        }

        [DisplayName("关键字")]
        public string Keywords
        {
            get;
            set;
        }

        [DisplayName("连接")]
        public string Link
        {
            get;
            set;
        }

        [DisplayName("创建时间")]
        public DateTime? DateCreated
        {
            get;
            set;
        }

      
        [DisplayName("文件路径")]
        public string FilePath
        {
            get;
            set;
        }
        [DisplayName("内容")]
        public string Description {get; set; }  
        public string Content { get; set; }  
    }

}
