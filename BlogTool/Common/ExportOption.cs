using System;
using System.Collections.Generic;
using System.Text;

namespace BlogTool.Common
{
    public class ExportOption
    {

        /// <summary>
        /// 图片高度（像素）
        /// </summary>
        public int Height { get; set; }


        /// <summary>
        /// 图片宽度（像素）
        /// </summary>
        public int Width { get; set; }
        public string Format { get; set; }
    }
}
