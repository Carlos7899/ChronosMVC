using ChronosMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

        #region POST - Candidatar-se a uma vaga
        [HttpPost]
        public async Task<IActionResult> Candidatar(int idVaga, CandidaturaModel candidatura)
        {
            try
            {
                // Chamar o endpoint da Vaga para buscar o idCorporacao
                int idCorporacao = await GetCorporacaoByVagaId(idVaga);

                // Atribuir o idCorporacao à candidatura
                candidatura.idCorporacao = idCorporacao;

                // Serializa o objeto candidatura
                string serializedCandidatura = JsonConvert.SerializeObject(candidatura);
                var content = new StringContent(serializedCandidatura, Encoding.UTF8, "application/json");

                // Envia a requisição POST para criar a candidatura
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl + "POST", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["MensagemSucesso"] = "Candidatura enviada com sucesso!";
                    return RedirectToAction("VagasView", "Vagas"); // Redireciona para a lista de vagas
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["MensagemErro"] = "Erro ao enviar candidatura: " + errorResponse;
                    return RedirectToAction("Detalhes", "Vagas", new { id = idVaga }); // Redireciona para a página da vaga
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro: " + ex.Message;
                return RedirectToAction("Detalhes", "Vagas", new { id = idVaga }); // Em caso de erro, retorna à página da vaga
            }
        }
        #endregion

        #region Método Auxiliar para Buscar o idCorporacao da Vaga
        private async Task<int> GetCorporacaoByVagaId(int idVaga)
        {
            try
            {
                // Chama o endpoint da Vaga para obter o idCorporacao
                HttpResponseMessage response = await _httpClient.GetAsync($"http://localhost:5027/api/Vaga/GetCorporacaoByVagaId/{idVaga}");
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
        #endregion
    }
}
