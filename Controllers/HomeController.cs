using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotNetCoreAppPostgres.Models;

namespace DotNetCoreAppPostgres.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly WeatherService _weatherService;

    public HomeController(ILogger<HomeController> logger, WeatherService weatherService)
    {
        _logger = logger;
        _weatherService = weatherService;
    }

    /// <summary>
    /// /[Authorize]
    /// </summary>
    /// <returns></returns>
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetWeather(string city = "Lahore")
    {
        var data = await _weatherService.GetWeatherAsync(city);
        if (data == null) return NotFound();
        return Json(data);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
