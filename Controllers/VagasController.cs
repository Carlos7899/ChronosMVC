using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ChronosMVC.Models;
using System.Text;

namespace ChronosMVC.Controllers
{
    public class VagasController : Controller
    {
        private readonly string uriBaseLocal = "http://localhost:5027/api/Vaga/";

        private readonly string uriBase = "http://Chronos.somee.com/ChronosApi/api/Vaga/";

        private readonly HttpClient _httpClient;

        public VagasController()
        {
            _httpClient = new HttpClient();
        }

        #region Ação para listar todas as vagas
        [HttpGet]
        public async Task<ActionResult> VagasView()
        {
            try
            {
                string uriComplementar = "GetAll";
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync(uriBase + uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var listaVagas = JsonConvert.DeserializeObject<List<VagaModel>>(serialized);
                    return View(listaVagas);
                }
                else
                    throw new Exception($"Erro: {response.StatusCode} - {serialized}");
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region Ação para buscar vagas por nome 
        [HttpGet]
        public async Task<IActionResult> PesquisarVagas(string nomeVaga)
        {
            try
            {
           
                string uriComplementar = $"GetByNome/{Uri.EscapeDataString(nomeVaga)}"; 
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await httpClient.GetAsync(uriBase + uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var vagas = JsonConvert.DeserializeObject<List<VagaModel>>(serialized);
                    if (vagas == null || !vagas.Any())
                    {
                        ViewBag.Mensagem = "Nenhuma vaga encontrada com esse nome.";
                    }
                    return View("VagasView", vagas); 
                }
                else
                {
                    throw new Exception(serialized);
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("VagasView");
            }
        }
        #endregion

        #region Ação para listar as vagas da corporação autenticada
        [HttpGet]
        public async Task<IActionResult> VagasPorCorporacao()
        {
            try
            {    
                var idCorporacao = GetAuthenticatedCorporacaoId();

                string uriComplementar = $"GetByCorporacao/{idCorporacao}";
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await httpClient.GetAsync(uriBase + uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var vagas = JsonConvert.DeserializeObject<List<VagaModel>>(serialized);
                    if (vagas == null || !vagas.Any())
                    {
                        ViewBag.Mensagem = "Você ainda não tem vagas.";
                    }
                    return View(vagas);
                }
                else
                {
                    throw new Exception(serialized);
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("VagasView"); 
            }
        }
        #endregion

        #region Ação para criar uma nova vaga
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateAsync(VagaModel vaga)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(vaga);
                }

                vaga.idCorporacao = GetAuthenticatedCorporacaoId();
                HttpClient httpClient = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(vaga));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await httpClient.PostAsync(uriBase + "POST", content);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    TempData["Mensagem"] = "Vaga criada com sucesso!";
                    return RedirectToAction("VagasView");
                }
                else
                {
                    throw new Exception(serialized);
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return View(vaga);
            }
        }
        #endregion

        #region Ação para ver detalhes da vaga
        [HttpGet]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); 
            }

            try
            {
                var vaga = await ObterVagaPorId(id.Value); 
                return View(vaga); 
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message; 
                return RedirectToAction("VagasView");
            }
        }
        #endregion

        #region Ação para editar uma vaga
        [HttpGet]
        public async Task<IActionResult> EditarVaga(int id)
        {
            try
            {
                var vagaAtual = await ObterVagaPorId(id);
                return View(vagaAtual);
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("VagasView");
            }
        }


        [HttpPost]
        public async Task<IActionResult> EditarVaga(VagaModel vaga)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var vagaAtual = await ObterVagaPorId(vaga.idVaga);
                    if (vagaAtual.idCorporacao != GetAuthenticatedCorporacaoId())
                    {
                        return Forbid("Você não tem permissão para editar esta vaga.");
                    }
 
                    var serializedVaga = JsonConvert.SerializeObject(vaga);
                    var content = new StringContent(serializedVaga, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PutAsync(uriBase + $"Put/{vaga.idVaga}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["MensagemSucesso"] = "Vaga editada com sucesso!";
                        return RedirectToAction("VagasView");
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
            return View(vaga);
        }

        #endregion

        #region Ação para deletar uma vaga
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var vagaAtual = await ObterVagaPorId(id);
                if (vagaAtual.idCorporacao != GetAuthenticatedCorporacaoId())
                {
                    return Forbid("Você não tem permissão para deletar esta vaga.");
                }

                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.DeleteAsync(uriBase + $"Delete/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    TempData["Mensagem"] = "Vaga deletada com sucesso!";
                    return RedirectToAction("VagasView");
                }
                else
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("VagasView");
            }
        }
        #endregion

        #region Métodos auxiliares
        private int GetAuthenticatedCorporacaoId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == "idCorporacao");
            if (idClaim != null && int.TryParse(idClaim.Value, out int idCorporacao))
            {
                return idCorporacao;
            }

            throw new Exception("Usuário não autenticado ou id da corporação não encontrado.");
        }

        private async Task<VagaModel> ObterVagaPorId(int id)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(uriBase + $"GetbyId/{id}");

            if (response.IsSuccessStatusCode)
            {
                string serialized = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VagaModel>(serialized);
            }

            throw new Exception("Vaga não encontrada.");
        }
        #endregion
    }
}
