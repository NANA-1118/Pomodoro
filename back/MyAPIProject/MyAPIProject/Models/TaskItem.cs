namespace MyAPIProject.Models
{
    public class TaskItem
    {
        public int TaskId { get; set; }
        public int MemberId { get; set; }
        public required string TaskName { get; set; }
        public bool IsDone { get; set; }
        public int TotalSeconds { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}