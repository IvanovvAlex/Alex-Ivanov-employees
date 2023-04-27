using Alex_Ivanov_employees.Controllers;
using Alex_Ivanov_employees.Data;
using Alex_Ivanov_employees.Models.TeamMember;
using System.IO;

namespace Alex_Ivanov_employees.Services.TeamMember
{
    public class TeamMemberService : ITeamMemberService
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment;

        public TeamMemberService(ILogger<HomeController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }
        public async Task<IEnumerable<EmployeePair>> GetPairOfMembers(IFormFile file)
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "Uploads", file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            var teamMembers = LoadMembersFromFile(filePath);

            IEnumerable<EmployeePair> result = FindLongestWorkingPair(teamMembers);

            return result;
        }

        private List<TeamMemberSchema> LoadMembersFromFile(string filePath)
        {
            var members = new List<TeamMemberSchema>();

            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    var member = new TeamMemberSchema
                    {
                        Id = values[0],
                        TeamId = values[1],
                        DateFrom = DateTime.Parse(values[2]),
                    };
                    if (!(string.IsNullOrEmpty(values[3]) || values[3].ToLower().Trim() == "null"))
                    {
                        member.DateTill = (DateTime)DateTime.Parse(values[3]);
                    }
                    else
                    {
                        member.DateTill = DateTime.UtcNow;

                    }

                    members.Add(member);
                }
            }

            return members;
        }
        public IEnumerable<EmployeePair> FindLongestWorkingPair(List<TeamMemberSchema> teamMembers)
        {
            List<EmployeePair> employeePairs = new List<EmployeePair>();
            List<string> checkedEmployeeIDs = new List<string>();


            for (int i = 0; i < teamMembers.Count - 1; i++)
            {
                if (checkedEmployeeIDs.Contains($"{teamMembers[i].Id}-{teamMembers[i].TeamId}"))
                {
                    continue;
                }
                for (int j = i + 1; j < teamMembers.Count; j++)
                {
                    if (checkedEmployeeIDs.Contains($"{teamMembers[j].Id}-{teamMembers[j].TeamId}"))
                    {
                        continue;
                    }
                    if (teamMembers[i].TeamId == teamMembers[j].TeamId && teamMembers[i].Id != teamMembers[j].Id)
                    {
                        DateTime startDate = teamMembers[i].DateFrom > teamMembers[j].DateFrom ? teamMembers[i].DateFrom : teamMembers[j].DateFrom;
                        DateTime? endDate = null;
                        if (teamMembers[i].DateTill != null && teamMembers[j].DateTill != null)
                        {
                            endDate = teamMembers[i].DateTill < teamMembers[j].DateTill ? teamMembers[i].DateTill : teamMembers[j].DateTill;
                        }
                        else if (teamMembers[i].DateTill != null)
                        {
                            endDate = teamMembers[i].DateTill;
                        }
                        else if (teamMembers[j].DateTill != null)
                        {
                            endDate = teamMembers[j].DateTill;
                        }
                        if (endDate != null)
                        {
                            TimeSpan duration = endDate.Value - startDate;
                            double days = Math.Abs(Math.Ceiling(duration.TotalDays));

                            EmployeePair employeePair = new EmployeePair()
                            {
                                FirstEmployeeID = teamMembers[i].Id,
                                SecondEmployeeID = teamMembers[j].Id,
                                TeamID = teamMembers[i].TeamId,
                                Duration = days
                            }
                            ;

                            if (!checkedEmployeeIDs.Contains($"{employeePair.FirstEmployeeID}-{employeePair.TeamID}")
                                && !checkedEmployeeIDs.Contains($"{employeePair.SecondEmployeeID}-{employeePair.TeamID}"))
                            {
                                employeePairs.Add(employeePair);
                                checkedEmployeeIDs.Add($"{employeePair.FirstEmployeeID}-{employeePair.TeamID}");
                                checkedEmployeeIDs.Add($"{employeePair.FirstEmployeeID}-{employeePair.TeamID}");
                            }
                        }
                    }
                }
            }

            return employeePairs;
        }
    }
}
