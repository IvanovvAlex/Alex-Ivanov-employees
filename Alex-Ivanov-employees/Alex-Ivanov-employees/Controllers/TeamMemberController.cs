using Alex_Ivanov_employees.Models;
using Microsoft.AspNetCore.Mvc;

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

            var filePath = Path.Combine(_environment.ContentRootPath, "uploads", file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            var projects = LoadMembersFromFile(filePath);

            var result = FindLongestWorkedProjects(projects);

            TeamMemberIndex model = new TeamMemberIndex(result);
            return View(model);
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
                        DateTo = string.IsNullOrEmpty(values[3]) ? null : (DateTime?)DateTime.Parse(values[3])
                    };

                    members.Add(member);
                }
            }

            return members;
        }

        private List<Tuple<string, string, string, TimeSpan>> FindLongestWorkedProjects(List<TeamMember> projects)
        {
            var pairs = GetEmployeePairs(projects);
            var longestProjects = new List<Tuple<string, string, string, TimeSpan>>();

            foreach (var pair in pairs)
            {
                var commonProjects = projects.Where(p => (p.EmpID == pair.Item1 && p.ProjectID == pair.Item2) || (p.EmpID == pair.Item2 && p.ProjectID == pair.Item1));

                if (commonProjects.Any())
                {
                    var longestProject = commonProjects.Aggregate((p1, p2) => (p2.DateTo ?? DateTimeOffset.Now) - p1.DateFrom > (p1.DateTo ?? DateTimeOffset.Now) - p2.DateFrom ? p2 : p1);

                    longestProjects.Add(new Tuple<string, string, string, TimeSpan>(pair.Item1, pair.Item2, longestProject.ProjectID, (longestProject.DateTo ?? DateTimeOffset.Now) - longestProject.DateFrom));
                }
            }

            return longestProjects;
        }

        private List<Tuple<string, string>> GetEmployeePairs(List<TeamMember> projects)
        {
            var employeeProjects = projects.GroupBy(p => p.EmpID);

            var pairs = new List<Tuple<string, string>>();

            foreach (var employeeProject in employeeProjects)
            {
                var projectsOfEmployee = employeeProject.Select(p => p.ProjectID).ToList();

                foreach (var otherEmployeeProject in employeeProjects.Where(ep => ep.Key != employeeProject.Key))
                {
                    var otherProjects = otherEmployeeProject.Select(p => p.ProjectID).ToList();

                    var commonProjects = projectsOfEmployee.Intersect(otherProjects);

                    if (commonProjects.Any())
                    {
                        var pair = new Tuple<string, string>(employeeProject.Key, otherEmployeeProject.Key);

                        pairs.Add(pair);
                    }
                }
            }

            return pairs;
        }
    }
}
