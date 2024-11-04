using ChronosMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace ChronosMVC.Controllers
{
    public class CandidaturaController : Controller
    {
        private readonly string apiUrl = "http://localhost:5027/api/Candidatura/";
        private readonly HttpClient _httpClient;

        public CandidaturaController()
        {
            _httpClient = new HttpClient();
        }


        #region Ação para listar todas as candidaturas
        [HttpGet]
        public async Task<ActionResult> ListaCandidaturas(int idEgresso)
        {
            try
            {
                string uriComplementar = $"GetByEgresso/{idEgresso}";
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl + uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var candidaturas = JsonConvert.DeserializeObject<List<CandidaturaModel>>(serialized);
                    return View(candidaturas);
                }
                else
                {
                    throw new Exception($"Erro: {response.StatusCode} - {serialized}");
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region Ação para criar uma nova candidatura
        [HttpGet]
        public ActionResult Create(int idVaga)
        {
            ViewBag.IdVaga = idVaga; // Passa o ID da vaga para a view
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(CandidaturaModel candidatura)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    candidatura.idEgresso = GetAuthenticatedEgressoId();
                    var content = new StringContent(JsonConvert.SerializeObject(candidatura), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PostAsync(apiUrl + "POST", content);
                    string serialized = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        TempData["MensagemSucesso"] = "Candidatura enviada com sucesso!";
                        return RedirectToAction("ListaCandidaturas", new { idEgresso = candidatura.idEgresso });
                    }
                    else
                    {
                        throw new Exception(serialized);
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = ex.Message;
                    return View(candidatura);
                }
            }
            return View(candidatura);
        }
        #endregion

        #region Ação para ver detalhes da candidatura
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var candidatura = await ObterCandidaturaPorId(id);
                return View(candidatura);
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("ListaCandidaturas", new { idEgresso = GetAuthenticatedEgressoId() });
            }
        }
        #endregion

        #region Ação para editar uma candidatura
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var candidatura = await ObterCandidaturaPorId(id);
                return View(candidatura);
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("ListaCandidaturas", new { idEgresso = GetAuthenticatedEgressoId() });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CandidaturaModel candidatura)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var content = new StringContent(JsonConvert.SerializeObject(candidatura), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await _httpClient.PutAsync(apiUrl + $"Put/{candidatura.idCandidatura}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["MensagemSucesso"] = "Candidatura editada com sucesso!";
                        return RedirectToAction("ListaCandidaturas", new { idEgresso = candidatura.idEgresso });
                    }
                    else
                    {
                        throw new Exception(await response.Content.ReadAsStringAsync());
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = ex.Message;
                }
            }
            return View(candidatura);
        }
        #endregion

        #region Ação para deletar uma candidatura
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync(apiUrl + $"Delete/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["MensagemSucesso"] = "Candidatura deletada com sucesso!";
                    return RedirectToAction("ListaCandidaturas", new { idEgresso = GetAuthenticatedEgressoId() });
                }
                else
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("ListaCandidaturas", new { idEgresso = GetAuthenticatedEgressoId() });
            }
        }
        #endregion

        #region Métodos auxiliares
        private int GetAuthenticatedEgressoId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == "idEgresso");
            if (idClaim != null && int.TryParse(idClaim.Value, out int idEgresso))
            {
                return idEgresso;
            }

            throw new Exception("Usuário não autenticado ou id do egresso não encontrado.");
        }

        private async Task<CandidaturaModel> ObterCandidaturaPorId(int id)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl + $"GetbyId/{id}");

            if (response.IsSuccessStatusCode)
            {
                string serialized = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CandidaturaModel>(serialized);
            }

            throw new Exception("Candidatura não encontrada.");
        }
        #endregion
    }
}
 