using ChronosMVC.Models.Curriculo;
using Microsoft.AspNetCore.Mvc;

public class CurriculoController : Controller
{



    [HttpGet]
    public IActionResult CurriculoView()
    {
        return View();
    }
}
