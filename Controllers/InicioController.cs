using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using RIK.Models;

namespace RIK.Controllers;

public class InicioController : Controller
{
    public IActionResult Index()
    {
        if(/*SESSION ESTÁ ACTIVA*/)
        {
            return View();
        }
        else
        {
            return RedirectToAction("Index", "Login");
        }
    }
}