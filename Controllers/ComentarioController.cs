using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChronosMVC.Models;

namespace ChronosMVC.Controllers
{
    public class ComentarioController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string apiUrl = "http://localhost:5027/api/Comentario/";

        public ComentarioController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

       

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComentarioModel comentario)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync($"{apiUrl}Post", comentario);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", new { idPublicacao = comentario.idPublicacao });
                }
                ModelState.AddModelError("", "Erro ao adicionar comentário.");
            }
            return View(comentario);
        }

    }
}
