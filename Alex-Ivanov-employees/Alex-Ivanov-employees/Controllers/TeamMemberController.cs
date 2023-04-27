using Alex_Ivanov_employees.Models.TeamMember;
using Alex_Ivanov_employees.Services.TeamMember;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Alex_Ivanov_employees.Controllers
{
    public class TeamMemberController : Controller
    {
        private readonly ITeamMemberService _teamMemberService;
        public TeamMemberController(ITeamMemberService teamMemberService)
        {
            _teamMemberService = teamMemberService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewData["Error"] = "Please select a file.";
                return View();
            }

            if (file.FileName.Split('.').Last().ToLower() != "csv")
            {
                ViewData["Error"] = "Please select a CSV file.";
                return View();
            }

            IEnumerable<EmployeePair> result = await _teamMemberService.GetPairOfMembers(file);

            return View(result);
        }

        
    }
}
