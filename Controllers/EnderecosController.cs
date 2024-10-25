using ChronosMVC.Models.Enderecos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace ChronosMVC.Controllers
{
    public class EnderecosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string apiLogradouroUrl = "http://localhost:5027/api/Logradouro/";
        private readonly string apiCorporacaoEnderecoUrl = "http://localhost:5027/api/CorporacaoEndereco/";

        public EnderecosController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult CreateEnderecoCorporacao()
        {
            return View(new EnderecoViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateLogradouro(EnderecoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Log os erros do ModelState para depuração
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine(error); // Ou use um logger
                }
                return View(model);
            }

            // Serializa o logradouro
            var logradouroJson = JsonConvert.SerializeObject(model.Logradouro);
            var logradouroContent = new StringContent(logradouroJson, Encoding.UTF8, "application/json");

            // Chama o método Post na API
            var logradouroResponse = await _httpClient.PostAsync(apiLogradouroUrl + "POST", logradouroContent);

            if (logradouroResponse.IsSuccessStatusCode)
            {
                var novoLogradouro = await logradouroResponse.Content.ReadAsStringAsync();
                var logradouroModel = JsonConvert.DeserializeObject<LogradouroModel>(novoLogradouro);

                model.CorporacaoEndereco.idLogradouro = logradouroModel.idLogradouro;

                // Serializa o endereço
                var enderecoJson = JsonConvert.SerializeObject(model.CorporacaoEndereco);
                var enderecoContent = new StringContent(enderecoJson, Encoding.UTF8, "application/json");

                // Chama o método Post para criar o endereço
                var enderecoResponse = await _httpClient.PostAsync(apiCorporacaoEnderecoUrl, enderecoContent);

                if (enderecoResponse.IsSuccessStatusCode)
                {
                    TempData["MensagemSucesso"] = "Endereço criado com sucesso!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Erro ao criar o endereço da corporação.");
                }
            }
            else
            {
                var errorContent = await logradouroResponse.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Erro ao criar o logradouro: {errorContent}");
                Console.WriteLine($"Erro ao criar logradouro: {errorContent}"); // Ou use um logger
            }

            return View(model);
        }
    }
}