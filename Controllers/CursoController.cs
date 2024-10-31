using ChronosMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json; // Adicione esta diretiva
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SeuProjeto.Controllers
{
    public class CursoController : Controller
    {
        private readonly string apiUrl = "http://localhost:5027/api/Curso/";
        private readonly HttpClient _httpClient;

        public CursoController()
        {
            _httpClient = new HttpClient();
        }

        #region Ação para buscar todos os cursos
        [HttpGet]
        public async Task<IActionResult> ListaCursos()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl + "GetAll");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync(); // Lê o conteúdo como string
                    var cursos = JsonConvert.DeserializeObject<IEnumerable<CursoModel>>(jsonResponse); // Desserializa
                    return View(cursos);
                }
                else
                {
                    // Tratamento de erro caso a API retorne um erro
                    ModelState.AddModelError(string.Empty, "Erro ao buscar cursos: " + response.ReasonPhrase);
                    return View(new List<CursoModel>());
                }
            }
            catch (Exception ex)
            {
                // Caso ocorra uma exceção, adicione o erro ao ModelState
                ModelState.AddModelError(string.Empty, "Erro ao buscar cursos: " + ex.Message);
                return View(new List<CursoModel>());
            }
        }
        #endregion
    }
}
