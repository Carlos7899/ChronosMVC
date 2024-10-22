using ChronosMVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

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
                var token = JsonConvert.DeserializeObject<TokenResponse>(responseContent).Token;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.emailEgresso),
                    new Claim("Token", token),
                    new Claim("idEgresso", model.idEgresso.ToString()), 
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


        public class TokenResponse
        {
            public string Token { get; set; }
        }
    }
}