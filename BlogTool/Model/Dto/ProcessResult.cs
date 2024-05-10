using System;

namespace BlogTool.Model.Dto
{
    public class ProcessResultDto
    {
        public ProcessResultDto(DateTime creationTime, string content)
        {
            this.CreationTime = creationTime;
            this.Content = content;
        }
        public int Id { get; set; }
        public DateTime CreationTime { get; set; }
        public string Content { get; set; }
    }
}