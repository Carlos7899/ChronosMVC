using ChronosMVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;
using static ChronosMVC.Controllers.CorporacaoController;

namespace ChronosMVC.Controllers
{
    public class EgressoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string apiUrl = "http://localhost:5027/api/Egresso/";

        public EgressoController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult LoginEgresso()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CadastroEgresso()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AdicionarDadosEgresso()
        {
            var id = User.FindFirst("idEgresso")?.Value;

            if (!string.IsNullOrEmpty(id))
            {
                var model = new EgressoModel { idEgresso = int.Parse(id) };
                return View(model);
            }

            TempData["MensagemErro"] = "Egresso não encontrado.";
            return RedirectToAction("Index", "Home");
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginEgresso(EgressoModel model)
        {
            var loginData = new { emailEgresso = model.emailEgresso, passwordString = model.PasswordString };
            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl + "Autenticar", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Aqui, você deve deserializar a resposta da API para obter o ID da corporação
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
                var token = tokenResponse.Token;
                var idEgresso = tokenResponse.IdEgresso; // Capturando o ID da Corporação
                Console.WriteLine($"ID do Egresso: {idEgresso}"); // Adicione esta linha


                // Configure a claims identity
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.emailEgresso),
                    new Claim("Token", token), // Adicione o token como uma claim
                    new Claim("idEgresso", idEgresso.ToString()), // Use o ID capturado
                    new Claim(ClaimTypes.Role, "Egresso") // Adicionando a role
                };

                var claimsIdentity = new ClaimsIdentity(claims, "login");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(claimsPrincipal); // Autentica o usuário

                TempData["MensagemSucesso"] = "Login realizado com sucesso!";
                return RedirectToAction("Index", "Home");
            }

            TempData["MensagemErro"] = $"Erro: {responseContent}";
            return View("LoginCorporacao", model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarEgresso(EgressoModel model)
        {

            var registerData = new { emailEgresso = model.emailEgresso, passwordString = model.PasswordString };
            var json = JsonConvert.SerializeObject(registerData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl + "Registrar", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                TempData["MensagemSucesso"] = "Registro realizado com sucesso!";
                return RedirectToAction("LoginEgresso"); 
            }

            TempData["MensagemErro"] = $"Erro: {responseContent}";
            return View(model); 
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarInformacoes(EgressoModel model)
        {
            // Certifique-se de que o idCorporacao está no modelo
            if (model.idEgresso <= 0)
            {
                TempData["MensagemErro"] = "ID do Egresso não é válido.";
                return View("AdicionarDadosEgresso", model);
            }

            // Serializa o modelo para JSON
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Envia o pedido para a API
            var response = await _httpClient.PutAsync(apiUrl + "Put/" + model.idEgresso, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Verifica se a resposta foi bem-sucedida
            if (response.IsSuccessStatusCode)
            {
                // Se a resposta contiver um corpo, tente deserializá-lo
                if (!string.IsNullOrEmpty(responseContent))
                {
                    try
                    {
                        var registeredEgresso = JsonConvert.DeserializeObject<EgressoModel>(responseContent);
                        TempData["idEgresso"] = registeredEgresso.idEgresso;
                        TempData["MensagemSucesso"] = "Informações atualizadas com sucesso!";
                    }
                    catch (JsonException ex)
                    {
                        // Se houver um erro ao deserializar, registre o erro
                        TempData["MensagemErro"] = $"Erro ao processar a resposta: {ex.Message}";
                    }
                }
                else
                {
                    TempData["MensagemSucesso"] = "Informações atualizadas com sucesso!";
                }
                return RedirectToAction("DadosEgresso", "Egresso");
            }

            // Se a resposta não for bem-sucedida, registre o erro
            TempData["MensagemErro"] = $"Erro: {responseContent}";
            return View("AdicionarDadosEgresso", model);
        }








        [HttpGet]
        public async Task<IActionResult> DadosEgresso()
        {
            var id = User.FindFirst("idEgresso")?.Value;

            if (string.IsNullOrEmpty(id))
            {
                TempData["MensagemErro"] = "Egresso não encontrado.";
                return RedirectToAction("Index", "Home");
            }

            var response = await _httpClient.GetAsync(apiUrl + "GetbyId/" + id);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);

                Console.WriteLine($"API Response: {responseContent}"); // Para depuração

                if (apiResponse?.value != null)
                {
                    return View(apiResponse.value);
                }
                else
                {
                    TempData["MensagemErro"] = "Dados do Egresso não encontrados.";
                    return RedirectToAction("Index", "Home");
                }
            }

            TempData["MensagemErro"] = "Erro ao buscar os dados do Egresso.";
            return RedirectToAction("Index", "Home");
        }














        [HttpGet]
        public async Task<IActionResult> EditarDadosEgresso(int idEgresso)
        {
           
            var response = await _httpClient.GetAsync(apiUrl + "GetbyId/" + idEgresso);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

              
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);

                // Verifica se o valor não é nulo
                if (apiResponse?.value != null)
                {
                
                    return View("AdicionarDadosEgresso", apiResponse.value);
                }
            }

            TempData["MensagemErro"] = "Erro ao carregar os dados do Egresso.";
            return RedirectToAction("Index");
        }




        public class ApiResponse
        {
            public object result { get; set; }
            public EgressoModel value { get; set; }
        }



        public class TokenResponse
        {
            public string Token { get; set; }
            public int IdEgresso { get; set; }
        }
    }
}