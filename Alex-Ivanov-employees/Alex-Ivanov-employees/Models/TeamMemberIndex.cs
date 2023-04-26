namespace Alex_Ivanov_employees.Models
{
    public class TeamMemberIndex
    {
        public TeamMemberIndex(List<Tuple<string, string, string, TimeSpan>> result)
        {
            Result = result;
        }
        public List<Tuple<string, string, string, TimeSpan>> Result { get; set; }

    }
}
