namespace Alex_Ivanov_employees.Models
{
    public class TeamMember
    {
        public string EmpID { get; set; }
        public string ProjectID { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

    }
}
