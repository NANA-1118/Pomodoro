namespace MyAPIProject.Models
{
    public class Member
    {
        public int MemberId { get; set; }
        public required string Account { get; set; }
        public required string Password { get; set; }
        public string? GoogleMemberNumber { get; set; }
        public string? LineMemberNumber { get; set; }
        public string? MemberName { get; set; }
        public bool Isverified { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}