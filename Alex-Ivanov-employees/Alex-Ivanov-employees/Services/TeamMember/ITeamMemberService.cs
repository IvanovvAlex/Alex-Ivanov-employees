using Alex_Ivanov_employees.Models.TeamMember;

namespace Alex_Ivanov_employees.Services.TeamMember
{
    public interface ITeamMemberService
    {
        public Task<IEnumerable<EmployeePair>> GetPairOfMembers(IFormFile file);

    }
}
