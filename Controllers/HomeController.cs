using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RIK.Models;

namespace RIK.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if(/*SESSION ESTÁ ACTIVA*/)
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }
        else
        {
            return RedirectToAction("Index", "Login");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
