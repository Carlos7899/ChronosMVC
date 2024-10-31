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
            // Verifica se o modelo é válido
            if (!ModelState.IsValid)
            {
                // Inspecionar os erros de validação
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage); // Exibir no console para depuração
                }

                // Adicionar uma mensagem de erro geral
                ModelState.AddModelError(string.Empty, "Por favor, corrija os erros abaixo.");

                // Retorna à view com o modelo e erros
                return View(model);
            }

            // Serializa o modelo em JSON
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Faz a chamada PUT para a API
                var response = await _httpClient.PutAsync(apiUrl + model.idEgresso, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Verifica se a chamada foi bem-sucedida
                if (response.IsSuccessStatusCode)
                {
                    TempData["MensagemSucesso"] = "Informações atualizadas com sucesso!";
                    return RedirectToAction("DadosEgresso");
                }

                TempData["MensagemErro"] = $"Erro ao atualizar informações: {responseContent}";
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro inesperado ao atualizar informações.";
                Console.WriteLine($"Erro: {ex.Message}"); // Exibir erro no console
            }

            // Retorna à view com o modelo, para corrigir os erros
            return View(model);
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