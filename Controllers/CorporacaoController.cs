using ChronosMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace ChronosMVC.Controllers
{
    public class CorporacaoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string apiUrl = "http://localhost:5027/api/Corporacao/";

        public CorporacaoController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult LoginCorporacao()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CadastroCorporacao()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(CorporacaoModel model)
        {
            var loginData = new { emailCorporacao = model.emailCorporacao, passwordString = model.PasswordString };
            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl + "Autenticar", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var token = JsonConvert.DeserializeObject<TokenResponse>(responseContent).Token;

                // Configure a claims identity
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.emailCorporacao),
                    new Claim("Token", token) // Adicione o token como uma claim
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
        public async Task<IActionResult> RegistrarCorporacao(CorporacaoModel model)
        {

            var registerData = new { emailCorporacao = model.emailCorporacao, passwordString = model.PasswordString };
            var json = JsonConvert.SerializeObject(registerData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl + "Registrar", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                TempData["MensagemSucesso"] = "Registro realizado com sucesso!";
                return RedirectToAction("LoginCorporacao"); // Redireciona para a página de login
            }

            TempData["MensagemErro"] = $"Erro: {responseContent}";
            return View(model); // Retorna à view com os dados e mensagem de erro
        }







        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(); // Desloga o usuário
            TempData["MensagemSucesso"] = "Logout realizado com sucesso!";
            return RedirectToAction("Index", "Home");
        }




        public class TokenResponse
        {
            public string Token { get; set; }
        }
    }
}