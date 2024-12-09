using ChronosMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ChronosMVC.Controllers
{
    public class CursoController : Controller
    {
        private readonly string apiUrlLocal = "http://localhost:5027/api/Curso/";

        private readonly string apiUrl = "http://Chronos.somee.com/ChronosApi/api/Curso/";

        private readonly HttpClient _httpClient;

        public CursoController()
        {
            _httpClient = new HttpClient();
        }

        //Certo
        #region Ação para buscar todos os cursos 
        [HttpGet]
        public async Task<IActionResult> ListaCursos()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl + "GetAll");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var cursos = JsonConvert.DeserializeObject<IEnumerable<CursoModel>>(jsonResponse);
                    return View(cursos);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Erro ao buscar cursos: " + response.ReasonPhrase);
                    return View(new List<CursoModel>());
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erro ao buscar cursos: " + ex.Message);
                return View(new List<CursoModel>());
            }
        }
        #endregion   

        //Certo
        #region Ação para criar um novo curso
        [HttpGet]
        public IActionResult CriarCurso()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CriarCurso(CursoModel curso)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Obter o ID da corporação autenticada
                    curso.idCorporacao = GetAuthenticatedCorporacaoId();

                    // Serializa o objeto do curso
                    string serializedCurso = JsonConvert.SerializeObject(curso);
                    var content = new StringContent(serializedCurso, Encoding.UTF8, "application/json");

                    // Envia a requisição para a API
                    HttpResponseMessage response = await _httpClient.PostAsync(apiUrl + "POST", content);

                    // Verifica se o curso foi criado com sucesso
                    if (response.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        TempData["MensagemSucesso"] = "Curso criado com sucesso!";
                        return RedirectToAction("ListaCursos");
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
            return View(curso);
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

        //certo
        #region Ação para listar os cursos da corporação autenticada
        [HttpGet]
        public async Task<IActionResult> CursosPorCorporacao()
        {
            try
            {
                // Obter o ID da corporação autenticada
                var idCorporacao = GetAuthenticatedCorporacaoId();

                // Chamar o método da API para obter os cursos
                string uriComplementar = $"GetByCorporacao/{idCorporacao}";
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await httpClient.GetAsync(apiUrl + uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var cursos = JsonConvert.DeserializeObject<List<CursoModel>>(serialized);
                    if (cursos == null || !cursos.Any())
                    {
                        ViewBag.Mensagem = "Você ainda não tem cursos.";
                    }
                    return View(cursos);
                }
                else
                {
                    throw new Exception(serialized);
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("ListaCursos");
            }
        }
        #endregion

        //certo
        #region Ação para buscar cursos por nome
        [HttpGet]
        public async Task<IActionResult> PesquisarCursos(string nomeCurso)
        {
            try
            {
                // Chamar o método da API para obter os cursos pelo nome
                string uriComplementar = $"GetByNome/{Uri.EscapeDataString(nomeCurso)}"; // Encode para garantir que o nome seja seguro na URL
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await httpClient.GetAsync(apiUrl + uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var cursos = JsonConvert.DeserializeObject<List<CursoModel>>(serialized);
                    if (cursos == null || !cursos.Any())
                    {
                        ViewBag.Mensagem = "Nenhum curso encontrado com esse nome.";
                    }
                    return View("ListaCursos", cursos); // Retorna a view com a lista de cursos
                }
                else
                {
                    throw new Exception(serialized);
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("ListaCursos");
            }
        }
        #endregion

        //certo
        #region Ação para editar um curso
        [HttpGet]
        public async Task<IActionResult> EditarCurso(int id)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl + $"GetbyId/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var curso = JsonConvert.DeserializeObject<CursoModel>(jsonResponse);
                    return View(curso);
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
                return RedirectToAction("ListaCursos");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarCurso(CursoModel curso)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Serializa o objeto do curso
                    string serializedCurso = JsonConvert.SerializeObject(curso);
                    var content = new StringContent(serializedCurso, Encoding.UTF8, "application/json");

                    // Envia a requisição para a API
                    HttpResponseMessage response = await _httpClient.PutAsync(apiUrl + $"Put/{curso.idCurso}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["MensagemSucesso"] = "Curso editado com sucesso!";
                        return RedirectToAction("ListaCursos");
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
            // Se o modelo não for válido, retorna a view com o modelo
            return View(curso);
        }
        #endregion

        //certo
        #region Ação para deletar um curso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync(apiUrl + $"Delete/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["MensagemSucesso"] = "Curso deletado com sucesso!";
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

            return RedirectToAction("ListaCursos");
        }
        #endregion

        //certo
        #region Ação para exibir os detalhes do curso
        [HttpGet]
        public async Task<IActionResult> DetalhesCurso(int id)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl + $"GetbyId/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var curso = JsonConvert.DeserializeObject<CursoModel>(jsonResponse);
                    return View(curso);  // Passa o curso para a view
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
                return RedirectToAction("ListaCursos");
            }
        }
        #endregion



    }
}
