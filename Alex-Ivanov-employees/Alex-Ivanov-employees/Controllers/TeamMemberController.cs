using Alex_Ivanov_employees.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Alex_Ivanov_employees.Controllers
{
    public class TeamMemberController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment;

        public TeamMemberController(ILogger<HomeController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewData["Error"] = "Please select a file.";
                return View();
            }

            var filePath = Path.Combine(_environment.ContentRootPath, "Uploads", file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            var teamMembers = LoadMembersFromFile(filePath);

            var result = FindLongestWorkedProjects(teamMembers);

            return View(result);
        }

        private List<TeamMember> LoadMembersFromFile(string filePath)
        {
            var members = new List<TeamMember>();

            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    var member = new TeamMember
                    {
                        EmpID = values[0],
                        ProjectID = values[1],
                        DateFrom = DateTime.Parse(values[2]),
                    };
                    if (!(string.IsNullOrEmpty(values[3]) || values[3].ToLower().Trim() == "null"))
                    { 
                        member.DateTo = (DateTime)DateTime.Parse(values[3]);
                    }

                    members.Add(member);
                }
            }

            return members;
        }

        private TeamMembersIndex FindLongestWorkedProjects(List<TeamMember> teamMembers)
        {
            // Identify the pair of employees who have worked together on common projects for the longest period of time
            int maxDuration = 0;
            string emp1 = string.Empty;
            string emp2 = string.Empty;
            for (int i = 0; i < teamMembers.Count - 1; i++)
            {
                for (int j = i + 1; j < teamMembers.Count; j++)
                {
                    if (teamMembers[i].ProjectID == teamMembers[j].ProjectID)
                    {
                        int duration = (int)(teamMembers[i].DateTo - teamMembers[j].DateFrom).TotalDays;
                        if (duration > maxDuration && teamMembers[i] != teamMembers[j])
                        {
                            maxDuration = duration;
                            emp1 = teamMembers[i].EmpID;
                            emp2 = teamMembers[j].EmpID;
                        }
                    }
                }
            }

            return new TeamMembersIndex {              
                Emp1 = emp1,
                Emp2 = emp2,
                Duration = maxDuration
            };

        }
    }
}
