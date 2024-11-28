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
        private readonly string apiUrlLocal = "http://localhost:5027/api/Comentario/";
        private readonly string apiUrl = "http://Chronos.somee.com/ChronosApi/api/Comentario/";
        public ComentarioController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region GET - Listar Comentários
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var comentarios = await _httpClient.GetFromJsonAsync<List<ComentarioModel>>(apiUrl + "GetAll");
            return View(comentarios);
        }
        #endregion

        #region GET - Detalhar Comentário
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var comentario = await _httpClient.GetFromJsonAsync<ComentarioModel>(apiUrl + $"GetById/{id}");
            if (comentario == null)
            {
                return NotFound();
            }
            return View(comentario);
        }
        #endregion

        #region Criar Comentário

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComentarioModel comentario)
        {
            // Validação do modelo
            if (!ModelState.IsValid)
            {
                return View(comentario);
            }

            // Preencher idEgresso
            comentario.idEgresso = GetAuthenticatedEgressoId();

            var response = await _httpClient.PostAsJsonAsync(apiUrl + "Post", comentario);
            if (response.IsSuccessStatusCode)
            {
                TempData["Mensagem"] = "Comentário criado com sucesso!";
                return RedirectToAction("Details", "Publicacao", new { id = comentario.idPublicacao }); // Redireciona para a view de detalhes da publicação
            }

            ModelState.AddModelError(string.Empty, "Erro ao criar comentário.");
            return View(comentario);
        }

        #endregion


        #region GET - Editar Comentário
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var comentario = await _httpClient.GetFromJsonAsync<ComentarioModel>(apiUrl + $"GetById/{id}");
            if (comentario == null)
            {
                return NotFound();
            }
            return View(comentario);
        }
        #endregion

        #region POST - Editar Comentário
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ComentarioModel comentario)
        {
            if (id != comentario.idComentario)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(comentario);
            }

            var response = await _httpClient.PutAsJsonAsync(apiUrl + $"{id}", comentario);
            if (response.IsSuccessStatusCode)
            {
                TempData["Mensagem"] = "Comentário editado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Erro ao editar comentário.");
            return View(comentario);
        }
        #endregion

        #region POST - Deletar Comentário
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync(apiUrl + $"Delete/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Mensagem"] = "Comentário deletado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            TempData["MensagemErro"] = "Erro ao deletar comentário.";
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Métodos auxiliares

        private int GetAuthenticatedEgressoId()
        {
            // Aqui você deve implementar a lógica para obter o idEgresso a partir da sessão ou do contexto do usuário
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == "idEgresso");
            if (idClaim != null && int.TryParse(idClaim.Value, out int idEgresso))
            {
                return idEgresso;
            }

            throw new Exception("Usuário não autenticado ou id do egresso não encontrado.");
        }

        #endregion
    }
}
