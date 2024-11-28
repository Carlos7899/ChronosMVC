using ChronosMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.Arm;

namespace ChronosMVC.Controllers
{
    public class PublicacaoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string apiUrlLocal = "http://localhost:5027/api/Publicacao/";

        private readonly string apiUrl = "http://Chronos.somee.com/ChronosApi/api/Publicacao/";

        public PublicacaoController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        #region Ação para listar todas as publicações
        [HttpGet]
        public async Task<IActionResult> ListarPublicacoes()
        {
            var publicacoes = await ObterPublicacoes();

            if (publicacoes == null)
            {
                return RedirectToAction("Index", "Home"); 
            }

            return View(publicacoes); 
        }

        private async Task<List<PublicacaoModel>> ObterPublicacoes()
        {
            try
            {
                string uriComplementar = "GetAll"; 
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario"); 
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); 

                HttpResponseMessage response = await httpClient.GetAsync(apiUrl + uriComplementar); 
                string serialized = await response.Content.ReadAsStringAsync(); 

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<List<PublicacaoModel>>(serialized); 
                }
                else
                {
                    throw new Exception(serialized); 
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message; 
                return null; 
            }
        }
        #endregion

        #region Ação para criar uma nova publicação
        [HttpGet]
        public IActionResult CriarPublicacao()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CriarPublicacao(PublicacaoModel publicacao)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Obter o ID da corporação autenticada
                    publicacao.idCorporacao = GetAuthenticatedCorporacaoId();

                    // Serialize o objeto de publicação
                    string serializedPublicacao = JsonConvert.SerializeObject(publicacao);
                    var content = new StringContent(serializedPublicacao, Encoding.UTF8, "application/json");

                    // Envia a requisição para a API
                    HttpResponseMessage response = await _httpClient.PostAsync(apiUrl + "POST", content);

                    // Verifica se a publicação foi criada com sucesso
                    if (response.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        TempData["MensagemSucesso"] = "Publicação criada com sucesso!";
                        return RedirectToAction("ListarPublicacoes");
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        throw new Exception(errorResponse);
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = ex.Message;
                }
            }

            // Se o modelo não for válido ou ocorreu um erro, retorna a view com o modelo
            return View(publicacao);
        }

        private int GetAuthenticatedCorporacaoId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == "idCorporacao");
            if (idClaim != null && int.TryParse(idClaim.Value, out int idCorporacao))
            {
                return idCorporacao;
            }

            throw new Exception("Usuário não autenticado ou id da corporação não encontrado.");
        }
        #endregion

        #region Ação para exibir os detalhes de uma publicação
        [HttpGet]
        public async Task<IActionResult> DetalhesPublicacao(int id)
        {
            try
            {
                string uriComplementar = $"GetbyId/{id}"; // Incluindo o ID na URL
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await httpClient.GetAsync(apiUrl + uriComplementar); // A URL agora contém o ID
                if (response.IsSuccessStatusCode)
                {
                    string serialized = await response.Content.ReadAsStringAsync();
                    var publicacao = JsonConvert.DeserializeObject<PublicacaoModel>(serialized);
                    return View(publicacao);
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception(errorResponse);
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("ListarPublicacoes");
            }
        }
        #endregion

        #region Ação para listar as publicações da corporação autenticada
        [HttpGet]
        public async Task<IActionResult> MinhasPublicacoes()
        {
            try
            {
                // Obter o ID da corporação autenticada
                var idCorporacao = GetAuthenticatedCorporacaoId();

                // Chamar o método da API para obter as publicações
                string uriComplementar = $"GetByCorporacao/{idCorporacao}";
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await httpClient.GetAsync(apiUrl + uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var publicacoes = JsonConvert.DeserializeObject<List<PublicacaoModel>>(serialized);
                    if (publicacoes == null || !publicacoes.Any())
                    {
                        ViewBag.Mensagem = "Você ainda não tem publicações.";
                    }
                    return View(publicacoes);
                }
                else
                {
                    throw new Exception(serialized);
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("ListarPublicacoes");
            }
        }
        #endregion


        #region Ação para deletar uma publicação
        [HttpPost]
        [ValidateAntiForgeryToken] // Adicione esta linha para proteção CSRF
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await httpClient.DeleteAsync(apiUrl + $"Delete/{id}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["MensagemSucesso"] = "Publicação deletada com sucesso!";
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception(errorResponse);
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;

            }

        
            return RedirectToAction("MinhasPublicacoes");
        }
        #endregion

        #region Ação para editar uma publicação
        [HttpGet]
        public async Task<IActionResult> EditarPublicacao(int id)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await httpClient.GetAsync(apiUrl + $"GetbyId/{id}");
                if (response.IsSuccessStatusCode)
                {
                    string serialized = await response.Content.ReadAsStringAsync();
                    var publicacao = JsonConvert.DeserializeObject<PublicacaoModel>(serialized);
                    return View(publicacao);
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception(errorResponse);
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("MinhasPublicacoes");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarPublicacao(PublicacaoModel publicacao)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string serializedPublicacao = JsonConvert.SerializeObject(publicacao);
                    var content = new StringContent(serializedPublicacao, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PutAsync(apiUrl + $"Put/{publicacao.idPublicacao}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["MensagemSucesso"] = "Publicação editada com sucesso!";
                        return RedirectToAction("MinhasPublicacoes");
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        throw new Exception(errorResponse);
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = ex.Message;
                }
            }

            return View(publicacao);
        }
        #endregion



    }
}