using Microsoft.AspNetCore.Mvc;
using EmployeeApp.Models;
using Newtonsoft.Json;

namespace EmployeeApp.Controllers
{
    public class HomeController : Controller
    {

        private readonly string apiUrl = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";

        public async Task<ActionResult> Index()
        {

            List<Employee>? employees = await GetEmployeesFromApi();

            if (employees == null)
            {
                ModelState.AddModelError(string.Empty, "Error fetching employees from API");
                return View();
            }

            List<SelectedEmployee> selectedEmployees = employees
                .Where(e => e != null)
                .GroupBy(e => e.EmployeeName)
                .Select(g => new SelectedEmployee
                {
                    EmployeeName = g.Key ?? "",
                    TotalTimeWorked = TimeSpan.FromTicks((long?)(g.Sum(e => e?.TotalTimeWorked?.Ticks) ?? 0) ?? 0)
                })
                .OrderBy(g => g.TotalTimeWorked)
                .ToList();

            return View(selectedEmployees);
        }


        [HttpGet]
        private async Task<List<Employee>?> GetEmployeesFromApi()
        {
            List<Employee>? employees = null;

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    if (data != null)
                    {
                        employees = JsonConvert.DeserializeObject<List<Employee>>(data);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Received null data from API");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error fetching data from API");
                }
            }

            return employees;
        }

    }
}