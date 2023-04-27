namespace Alex_Ivanov_employees.Models
{
    public class TeamMember
    {
        public string Id { get; set; }
        public string TeamId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTill { get; set; }

    }
}
