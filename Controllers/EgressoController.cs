using ChronosMVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System;

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

        #region Login e Registro de Egresso
        [HttpGet]
        public IActionResult LoginEgresso()
        {
            return View();
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
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
                var token = tokenResponse.Token;
                var idEgresso = tokenResponse.IdEgresso;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.emailEgresso),
                    new Claim("Token", token),
                    new Claim("idEgresso", idEgresso.ToString()),
                    new Claim(ClaimTypes.Role, "Egresso")
                };

                var claimsIdentity = new ClaimsIdentity(claims, "login");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(claimsPrincipal);

                TempData["MensagemSucesso"] = "Login realizado com sucesso!";
                return RedirectToAction("Index", "Home");
            }

            TempData["MensagemErro"] = $"Erro: {responseContent}";
            return View("LoginEgresso", model);
        }

        [HttpGet]
        public IActionResult CadastroEgresso()
        {
            return View();
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
        #endregion

        #region Manipulação de Dados do Egresso
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
        public async Task<IActionResult> AdicionarInformacoes(EgressoModel model)
        {
            try
            {
                // Obter o ID do Egresso logado (via claim)
                var idEgresso = GetLoggedEgressoId();

                // Verifica se o idEgresso do modelo é igual ao idEgresso logado
                if (model.idEgresso != idEgresso)
                {
                    TempData["MensagemErro"] = "ID do egresso não corresponde ao egresso logado.";
                    return RedirectToAction("Index", "Home");
                }

                // Serializa o modelo para JSON
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Envia os dados para a API (exemplo: para salvar ou atualizar as informações do Egresso)
                var response = await _httpClient.PutAsync(apiUrl + "Put/" + model.idEgresso, content); // Ajustado!
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["MensagemSucesso"] = "Informações atualizadas com sucesso!";
                    return RedirectToAction("DadosEgresso", "Egresso");
                }

                TempData["MensagemErro"] = $"Erro: {responseContent}";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = $"Erro inesperado: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
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

                if (apiResponse?.value != null)
                {
                    return View("AdicionarDadosEgresso", apiResponse.value);
                }
            }

            TempData["MensagemErro"] = "Erro ao carregar os dados do Egresso.";
            return RedirectToAction("Index");
        }
        #endregion

        #region Modelos de Respostas
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
        #endregion

        private int GetLoggedEgressoId()
        {
            // Exemplo, recuperando o id do egresso do claim ou sessão, dependendo do seu contexto
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == "idEgresso");
            if (idClaim != null && int.TryParse(idClaim.Value, out int idEgresso))
            {
                return idEgresso;
            }

            throw new Exception("Egresso não autenticado.");
        }
    }
}
