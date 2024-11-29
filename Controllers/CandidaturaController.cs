using ChronosMVC.Models;
using ChronosMVC.Models.Curriculo;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChronosMVC.Controllers
{
    public class CandidaturaController : Controller
    {
        private readonly string apiCurriculoUrlLocal = "http://localhost:5027/api/Curriculo/";
        private readonly string apiExperienciaUrlLocal = "http://localhost:5027/api/Experiencia/";
        private readonly string apiFormacaoUrlLocal = "http://localhost:5027/api/Formacao/";
        private readonly string apiUrlLocal = "http://localhost:5027/api/Candidatura/";

        private readonly string apiCurriculoUrl = "http://Chronos.somee.com/ChronosApi/api/Curriculo/";
        private readonly string apiExperienciaUrl = "http://Chronos.somee.com/ChronosApi/api/Experiencia/";
        private readonly string apiFormacaoUrl = "http://Chronos.somee.com/ChronosApi/api/Formacao/";
        private readonly string apiUrl = "http://Chronos.somee.com/ChronosApi/api/Candidatura/";

        private readonly HttpClient _httpClient;

        public CandidaturaController()
        {
            _httpClient = new HttpClient();
        }

        #region GET
        [HttpGet]
        public async Task<IActionResult> CandidaturasPorCorporacao()
        {
            try
            {
                int idCorporacao = GetLoggedCorporacaoId();

                HttpResponseMessage response = await _httpClient.GetAsync($"{apiUrl}GetByCorporacao/{idCorporacao}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var candidaturas = JsonConvert.DeserializeObject<List<CandidaturaModel>>(jsonResponse);

                    return View(candidaturas);
                }
                else
                {
                    TempData["MensagemErro"] = "Erro ao carregar as candidaturas.";
                    return View(new List<CandidaturaModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro: " + ex.Message;
                return View(new List<CandidaturaModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> MinhasCandidaturas()
        {
            try
            {
                int idEgresso = GetLoggedEgressoId();

                HttpResponseMessage response = await _httpClient.GetAsync($"{apiUrl}GetByEgresso/{idEgresso}");
                if (response.IsSuccessStatusCode)
                {

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var candidaturas = JsonConvert.DeserializeObject<List<CandidaturaModel>>(jsonResponse);

                    return View(candidaturas);
                }
                else
                {
                    TempData["MensagemErro"] = "Erro ao carregar suas candidaturas.";
                    return View(new List<CandidaturaModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro: " + ex.Message;
                return View(new List<CandidaturaModel>());
            }
        }

        #endregion

        #region PUT
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{apiUrl}GetById/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var candidatura = JsonConvert.DeserializeObject<CandidaturaModel>(jsonResponse);

                    if (candidatura == null)
                    {
                        TempData["MensagemErro"] = "Candidatura não encontrada.";
                        return RedirectToAction("CandidaturasPorCorporacao");
                    }

                    return View(candidatura);
                }
                else
                {
                    TempData["MensagemErro"] = $"Erro ao carregar a candidatura. Código: {response.StatusCode}";
                    return RedirectToAction("CandidaturasPorCorporacao");
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro inesperado: " + ex.Message;
                return RedirectToAction("CandidaturasPorCorporacao");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Editar(CandidaturaModel candidatura)
        {
            try
            {
               
                if (candidatura.idCandidatura <= 0)
                {
                    TempData["MensagemErro"] = "Candidatura inválida.";
                    return View(candidatura);
                }

                string serializedCandidatura = JsonConvert.SerializeObject(candidatura);
                var content = new StringContent(serializedCandidatura, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PutAsync($"{apiUrl}PUT/{candidatura.idCandidatura}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["MensagemSucesso"] = "Candidatura atualizada com sucesso.";
                    return RedirectToAction("CandidaturasPorCorporacao");
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["MensagemErro"] = "Erro ao atualizar a candidatura: " + errorResponse;
                    return View(candidatura);
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro ao tentar atualizar a candidatura: " + ex.Message;
                return View(candidatura);
            }
        }





        #endregion

        #region POST - Candidatar-se a uma vaga
        [HttpPost]
        public async Task<IActionResult> Candidatar(int idVaga, CandidaturaModel candidatura)
        {
            try
            {
                int idEgresso = GetLoggedEgressoId();
                candidatura.idEgresso = idEgresso;

                int idCorporacao = await GetCorporacaoByVagaId(idVaga);

                candidatura.idCorporacao = idCorporacao;

                string serializedCandidatura = JsonConvert.SerializeObject(candidatura);
                var content = new StringContent(serializedCandidatura, Encoding.UTF8, "application/json");


                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl + "POST", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["MensagemSucesso"] = "Candidatura enviada com sucesso!";
                    return RedirectToAction("VagasView", "Vagas");
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["MensagemErro"] = "Erro ao enviar candidatura: " + errorResponse;
                    return RedirectToAction("Detalhes", "Vagas", new { id = idVaga });
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro: " + ex.Message;
                return RedirectToAction("Detalhes", "Vagas", new { id = idVaga });
            }
        }
        #endregion

        #region get para ver o curriculo 
        [HttpGet]
        public async Task<IActionResult> VerCurriculo(int idCandidatura)
        {
            try
            {
                // Recupera a candidatura pelo ID (assumindo que ela contém o ID do egresso)
                var apiCandidaturaEndpoint = apiUrl + $"GetById/{idCandidatura}";
                HttpResponseMessage responseCandidatura = await _httpClient.GetAsync(apiCandidaturaEndpoint);

                if (responseCandidatura.IsSuccessStatusCode)
                {
                    var jsonCandidatura = await responseCandidatura.Content.ReadAsStringAsync();
                    var candidatura = JsonConvert.DeserializeObject<CandidaturaModel>(jsonCandidatura);

                    if (candidatura != null)
                    {
                        // Obter o ID do Egresso da candidatura
                        var idEgresso = candidatura.idEgresso;

                        // Carregar o currículo do egresso
                        var apiCurriculoEndpoint = apiCurriculoUrl + $"GetByEgresso/{idEgresso}";
                        HttpResponseMessage responseCurriculo = await _httpClient.GetAsync(apiCurriculoEndpoint);

                        if (responseCurriculo.IsSuccessStatusCode)
                        {
                            var jsonCurriculo = await responseCurriculo.Content.ReadAsStringAsync();
                            var curriculo = JsonConvert.DeserializeObject<CurriculoModel>(jsonCurriculo);

                            if (curriculo != null)
                            {
                                // Carregar as experiências profissionais e formações acadêmicas de forma paralela
                                var apiExperienciaEndpoint = apiExperienciaUrl + $"GetByCurriculo/{curriculo.idCurriculo}";
                                var apiFormacaoEndpoint = apiFormacaoUrl + $"GetByCurriculo/{curriculo.idCurriculo}";

                                // Usando `Task.WhenAll` para executar as requisições em paralelo
                                var experienciaTask = _httpClient.GetAsync(apiExperienciaEndpoint);
                                var formacaoTask = _httpClient.GetAsync(apiFormacaoEndpoint);

                                // Espera por todas as requisições
                                await Task.WhenAll(experienciaTask, formacaoTask);

                                // Tratando as respostas das requisições
                                if (experienciaTask.Result.IsSuccessStatusCode)
                                {
                                    var jsonExperiencia = await experienciaTask.Result.Content.ReadAsStringAsync();
                                    curriculo.ExperienciasProfissionais = JsonConvert.DeserializeObject<List<ExperienciaModel>>(jsonExperiencia);
                                }

                                if (formacaoTask.Result.IsSuccessStatusCode)
                                {
                                    var jsonFormacao = await formacaoTask.Result.Content.ReadAsStringAsync();
                                    curriculo.FormacoesAcademicas = JsonConvert.DeserializeObject<List<FormacaoModel>>(jsonFormacao);
                                }

                                // Carregar o nome do egresso
                                ViewBag.nomeEgresso = await GetNomeEgresso(idEgresso);

                                // Passar o currículo, experiências e formações para a view
                                return View(curriculo);
                            }
                            else
                            {
                                TempData["MensagemErro"] = "Currículo não encontrado.";
                            }
                        }
                        else
                        {
                            string errorResponse = await responseCurriculo.Content.ReadAsStringAsync();
                            TempData["MensagemErro"] = "Erro ao carregar currículo: " + errorResponse;
                        }
                    }
                    else
                    {
                        TempData["MensagemErro"] = "Candidatura não encontrada.";
                    }
                }
                else
                {
                    string errorResponse = await responseCandidatura.Content.ReadAsStringAsync();
                    TempData["MensagemErro"] = "Erro ao carregar candidatura: " + errorResponse;
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro ao carregar informações: " + ex.Message;
            }

            // Retorna uma view vazia ou com uma mensagem de erro caso algo falhe
            return View(new CurriculoModel());
        }
        #endregion

        #region Métodos Auxiliares
        private async Task<int> GetCorporacaoByVagaId(int idVaga)
        {
            try
            {

                HttpResponseMessage response = await _httpClient.GetAsync($"http://Chronos.somee.com/ChronosApi/api/Vaga/GetCorporacaoByVagaId/{idVaga}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<int>(jsonResponse);
                }
                else
                {
                    throw new Exception("Vaga não encontrada ou erro ao obter o idCorporacao.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar idCorporacao da vaga: " + ex.Message);
            }
        }
        private int GetLoggedEgressoId()
        {

            var idClaim = User.Claims.FirstOrDefault(c => c.Type == "idEgresso");
            if (idClaim != null && int.TryParse(idClaim.Value, out int idEgresso))
            {
                return idEgresso;
            }

            throw new Exception("Egresso não autenticado.");
        }
        private int GetLoggedCorporacaoId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == "idCorporacao");
            if (idClaim != null && int.TryParse(idClaim.Value, out int idCorporacao))
            {
                return idCorporacao;
            }

            throw new Exception("Corporação não autenticada.");
        }

        private async Task<string> GetNomeEgresso(int idEgresso)
        {
            try
            {
                // Endpoint da API para buscar o egresso pelo ID
                var apiEgressoEndpoint = $"http://Chronos.somee.com/ChronosApi/api/Egresso/GetbyId/{idEgresso}";

                // Faz a requisição para a API
                HttpResponseMessage response = await _httpClient.GetAsync(apiEgressoEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // Deserializa a resposta JSON para o tipo ApiResponse
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);

                    // Verifica se a resposta possui um egresso válido e retorna o nome
                    if (apiResponse != null && apiResponse.value != null)
                    {
                        return apiResponse.value.nomeEgresso ?? "Nome não encontrado";
                    }
                    else
                    {
                        return "Nome não encontrado";
                    }
                }
                else
                {
                    throw new Exception($"Erro ao buscar nome do egresso: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna uma mensagem mais detalhada
                return $"Erro ao obter nome do egresso: {ex.Message}";
            }
        }



        public class EgressoResponse
        {
            public string nomeEgresso { get; set; }

        }

        public class ApiResponse
        {
            public object result { get; set; }
            public EgressoModel value { get; set; }
        }

        #endregion
    }
}
