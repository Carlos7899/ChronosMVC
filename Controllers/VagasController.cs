using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ChronosMVC.Models;

namespace ChronosMVC.Controllers
{
    public class VagasController : Controller
    {
        private readonly string uriBase = "http://localhost:5027/api/Vaga/";


        //certo
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


        #region Ação para listar as vagas da corporação autenticada
        [HttpGet]
        public async Task<IActionResult> VagasPorCorporacao()
        {
            try
            {
                // Obter o ID da corporação autenticada
                var idCorporacao = GetAuthenticatedCorporacaoId();

                // Chamar o método da API para obter as vagas
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
                return RedirectToAction("Index"); // Redireciona para a página inicial de vagas
            }
        }
        #endregion

        //certo
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
                    return RedirectToAction("Index");
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
        public async Task<ActionResult> DetailsAsync(int? id)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync(uriBase + id.ToString());
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var vaga = JsonConvert.DeserializeObject<VagaModel>(serialized);
                    return View(vaga);
                }
                else
                    throw new Exception(serialized);
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region Ação para editar uma vaga
        [HttpGet]
        public async Task<ActionResult> EditAsync(int id)
        {
            try
            {
                var vagaAtual = await ObterVagaPorId(id);
                return View(vagaAtual);
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditAsync(VagaModel vaga)
        {
            try
            {
                var vagaAtual = await ObterVagaPorId(vaga.idVaga);
                if (vagaAtual.idCorporacao != GetAuthenticatedCorporacaoId())
                {
                    return Forbid("Você não tem permissão para editar esta vaga.");
                }

                HttpClient httpClient = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(vaga));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await httpClient.PutAsync(uriBase + $"Put/{vaga.idVaga}", content);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    TempData["Mensagem"] = "Vaga editada com sucesso!";
                    return RedirectToAction("Index");
                }
                else
                    throw new Exception(serialized);
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return View(vaga);
            }
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
                    return RedirectToAction("Index");
                }
                else
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("Index");
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
