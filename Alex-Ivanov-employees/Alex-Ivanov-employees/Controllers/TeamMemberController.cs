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

            var result = FindLongestWorkingPair(teamMembers);

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
                        Id = values[0],
                        TeamId = values[1],
                        DateFrom = DateTime.Parse(values[2]),
                    };
                    if (!(string.IsNullOrEmpty(values[3]) || values[3].ToLower().Trim() == "null"))
                    {
                        member.DateTill = (DateTime)DateTime.Parse(values[3]);
                    }

                    members.Add(member);
                }
            }

            return members;
        }
        public EmployeePair FindLongestWorkingPair(List<TeamMember> teamMembers)
        {
            EmployeePair longestPair = null;
            Dictionary<string, double> employeePairs = new Dictionary<string, double>();

            for (int i = 0; i < teamMembers.Count - 1; i++)
            {
                for (int j = i + 1; j < teamMembers.Count; j++)
                {
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
                            double days = Math.Abs(duration.TotalDays);

                            string key = $"{teamMembers[i].Id}-{teamMembers[j].Id}";
                            if (employeePairs.ContainsKey(key))
                            {
                                if (days > employeePairs[key])
                                {
                                    employeePairs[key] = days;
                                }
                            }
                            else
                            {
                                employeePairs.Add(key, days);
                            }
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, double> pair in employeePairs)
            {
                string[] ids = pair.Key.Split('-');
                EmployeePair currentPair = new EmployeePair
                {
                    FirstEmployeeID = ids[0],
                    SecondEmployeeID = ids[1],
                    Duration = pair.Value
                };
                if (longestPair == null || currentPair.Duration > longestPair.Duration)
                {
                    longestPair = currentPair;
                }
            }

            return longestPair;
        }



    }
}
