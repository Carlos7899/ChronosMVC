using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ChronosMVC.Models;

namespace ChronosMVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Candidaturas()
    {
        return View();
    }

    public IActionResult Cadastro()
    {
        return View();
    }

    public IActionResult LoginView()
    {
        return View();
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
