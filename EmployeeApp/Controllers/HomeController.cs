using Microsoft.AspNetCore.Mvc;
using EmployeeApp.Models;
using Newtonsoft.Json;
using SkiaSharp;

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
        public async Task<IActionResult> GeneratePieChart()
        {
            List<Employee> employees = await GetEmployeesFromApi();

            if (employees == null)
            {
                ModelState.AddModelError(string.Empty, "Error fetching employees from API");
                return View();
            }

            var data = employees
                .Where(e => e != null)
                .GroupBy(e => e.EmployeeName)
                .Select(g => new SelectedEmployee
                {
                    EmployeeName = g.Key ?? "",
                    TotalTimeWorked = TimeSpan.FromTicks((long?)(g.Sum(e => e?.TotalTimeWorked?.Ticks) ?? 0) ?? 0)
                })
                .OrderBy(g => g.TotalTimeWorked)
                .ToList();

            using (var surface = SKSurface.Create(new SKImageInfo(400, 400)))
            {
                var canvas = surface.Canvas;

                canvas.Clear(SKColors.White);

                var totalHours = data.Sum(item => item.TotalTimeWorked.TotalHours);

                var startAngle = 0.0f;

                for (int i = 0; i < data.Count; i++)
                {
                    var ratio = (float)(data[i].TotalTimeWorked.TotalHours / totalHours);
                    var sweepAngle = ratio * 360.0f;

                    using (var paint = new SKPaint
                    {
                        Style = SKPaintStyle.Fill,
                        Color = SKColor.Parse(GetRandomColor()), 
                        IsAntialias = true
                    })
                    {
                        var rect = new SKRect(0, 0, 400, 400);
                        canvas.DrawArc(rect, startAngle, sweepAngle, true, paint);
                    }

                    // Add label with employee name and percentage
                    var labelAngle = startAngle + sweepAngle / 2;
                    var labelX = (float)(200 + 180 * Math.Cos(labelAngle * Math.PI / 180));
                    var labelY = (float)(200 + 180 * Math.Sin(labelAngle * Math.PI / 180));

                    using (var textPaint = new SKPaint
                    {
                        TextSize = 10,
                        TextAlign = SKTextAlign.Center,
                        Color = SKColors.Black,
                    })
                    {
                        var percentage = (int)(ratio * 100);
                        var name = data[i].EmployeeName;

                        var nameY = labelY - 8; 
                        var percentageY = labelY + 7; 

                        canvas.DrawText(name, labelX, nameY, textPaint);
                        canvas.DrawText($"{percentage}%", labelX, percentageY, textPaint);
                    }

                    startAngle += sweepAngle;
                }

                var image = surface.Snapshot();

                using (var memoryStream = new MemoryStream())
                {
                    image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(memoryStream);
                    memoryStream.Position = 0;

                    return File(memoryStream.ToArray(), "image/png");
                }
            }
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
        private string GetRandomColor()
        {
            // Generating a random color in hex format
            Random random = new Random();
            return String.Format("#{0:X6}", random.Next(0x1000000));
        }

    }
}