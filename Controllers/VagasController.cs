using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http;
using ChronosMVC.Models;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using ChronosMVC.Models;

namespace ChronosMVC.Controllers
{
    public class VagasController : Controller
    {
        public string uriBase = "http://localhost:5027/api/Vaga/";


        [HttpGet]
        public async Task<ActionResult> IndexAsync()
        {
            try
            {
                string uriComplementar = "GetAll"; // Endpoint da API
                HttpClient httpClient = new HttpClient();

                HttpResponseMessage response = await httpClient.GetAsync(uriBase + uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();

                // Registre a resposta
                Console.WriteLine(serialized); // Para depuração

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    List<VagaModel> listaTarefas = JsonConvert.DeserializeObject<List<VagaModel>>(serialized);
                    return View(listaTarefas);
                }
                else
                    throw new System.Exception($"Erro: {response.StatusCode} - {serialized}");
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }



        [HttpPost]
        public async Task<ActionResult> CreateAsync(VagaModel v)
        {
            try
            {
                // Validação do modelo
                if (!ModelState.IsValid)
                {
                    return View(v); // Retorna a mesma view com os dados preenchidos
                }

                // Obter o ID da corporação autenticada
                v.idCorporacao = GetAuthenticatedCorporacaoId(); // Define o idCorporacao para a vaga

                HttpClient httpClient = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(v));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // Enviar o POST para a API
                HttpResponseMessage response = await httpClient.PostAsync(uriBase + "POST", content);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    TempData["Mensagem"] = "Vaga criada com sucesso!";
                    return RedirectToAction("Index"); // Redireciona para a página principal de vagas
                }
                else
                {
                    throw new Exception(serialized);
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return View(v); // Retorna à view com os dados
            }
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




        [HttpGet]
        public async Task<ActionResult> DetailsAsync(int? id)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await httpClient.GetAsync(uriBase + id.ToString());
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    VagaModel v = await Task.Run(() =>
                    JsonConvert.DeserializeObject<VagaModel>(serialized));
                    return View(v);
                }
                else
                    throw new System.Exception(serialized);
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditAsync(VagaModel v)
        {
            try
            {
                // Verifica se a vaga pertence à corporação autenticada
                var vagaAtual = await ObterVagaPorId(v.idVaga); // Método que busca a vaga pelo ID
                if (vagaAtual.idCorporacao != GetAuthenticatedCorporacaoId())
                {
                    return Forbid("Você não tem permissão para editar esta vaga.");
                }

                HttpClient httpClient = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(v));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await httpClient.PutAsync(uriBase + $"Put/{v.idVaga}", content);
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
                return View(v); // Retorna à view com os dados
            }
        }

        [HttpGet]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            try
            {
                var vagaAtual = await ObterVagaPorId(id); // Método que busca a vaga pelo ID
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

        private async Task<VagaModel> ObterVagaPorId(int id)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(uriBase + $"GetbyId/{id}");

            if (response.IsSuccessStatusCode)
            {
                string serialized = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VagaModel>(serialized);
            }

            // Lidar com o erro se a vaga não for encontrada
            throw new Exception("Vaga não encontrada.");
        }


        [HttpGet]
        private async Task<List<VagaModel>> ObterVagas()
        {
            try
            {
                string uriComplementar = "GetAll";
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await httpClient.GetAsync(uriBase + uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<List<VagaModel>>(serialized);
                }
                else
                {
                    throw new Exception(serialized);
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return null; // Retorna nulo em caso de erro
            }
        }




        public async Task<IActionResult> VagasView()
        {
            var model = await ObterVagas(); // Chame o método de forma assíncrona
            return View(model ?? new List<VagaModel>()); // Garante que um modelo válido seja passado
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }






    }
}