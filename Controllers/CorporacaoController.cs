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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChronosMVC.Controllers
{
    public class CorporacaoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string apiUrlLocal = "http://localhost:5027/api/Corporacao/";

        private readonly string apiUrl = "http://Chronos.somee.com/ChronosApi/api/Corporacao/";

        public CorporacaoController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        #region Chama a view
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

        [HttpGet]
        public IActionResult AdicionarDadosCorporacao()
        {
            var id = User.FindFirst("idCorporacao")?.Value;

            if (!string.IsNullOrEmpty(id))
            {
                var model = new CorporacaoModel { idCorporacao = int.Parse(id) };
                return View(model);
            }

            TempData["MensagemErro"] = "Corporação não encontrada.";
            return RedirectToAction("Index", "Home");
        }
        #endregion






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
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
                var token = tokenResponse.Token;
                var idCorporacao = tokenResponse.IdCorporacao; 
                Console.WriteLine($"ID da Corporação: {idCorporacao}"); 


               
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.emailCorporacao),
                    new Claim("Token", token), 
                    new Claim("idCorporacao", idCorporacao.ToString()), 
                    new Claim(ClaimTypes.Role, "Corporacao"), 
                  
                };

                var claimsIdentity = new ClaimsIdentity(claims, "login");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(claimsPrincipal); 

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
                return RedirectToAction("LoginCorporacao"); 
            }

            TempData["MensagemErro"] = $"Erro: {responseContent}";
            return View(model); 
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarInformacoes(CorporacaoModel model)
        {
           
            if (model.idCorporacao <= 0)
            {
                TempData["MensagemErro"] = "ID da corporação não é válido.";
                return View("AdicionarDadosCorporacao", model);
            }

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(apiUrl + "Put/" + model.idCorporacao, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrEmpty(responseContent))
                {
                    try
                    {
                        var registeredCorporacao = JsonConvert.DeserializeObject<CorporacaoModel>(responseContent);
                        TempData["idCorporacao"] = registeredCorporacao.idCorporacao;
                        TempData["MensagemSucesso"] = "Informações atualizadas com sucesso!";
                    }
                    catch (JsonException ex)
                    {
                        TempData["MensagemErro"] = $"Erro ao processar a resposta: {ex.Message}";
                    }
                }
                else
                {
                    TempData["MensagemSucesso"] = "Informações atualizadas com sucesso!";
                }
                return RedirectToAction("DadosCorporacao", "Corporacao");
            }

            TempData["MensagemErro"] = $"Erro: {responseContent}";
            return View("AdicionarDadosCorporacao", model);
        }








        [HttpGet]
        public async Task<IActionResult> DadosCorporacao()
        {
            var id = User.FindFirst("idCorporacao")?.Value;

            if (string.IsNullOrEmpty(id))
            {
                TempData["MensagemErro"] = "Corporação não encontrada.";
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
                    TempData["MensagemErro"] = "Dados da corporação não encontrados.";
                    return RedirectToAction("Index", "Home");
                }
            }

            TempData["MensagemErro"] = "Erro ao buscar os dados da corporação.";
            return RedirectToAction("Index", "Home");
        }













        [HttpGet]
        public async Task<IActionResult> EditarDadosCorporacao(int idCorporacao)
        {
            var response = await _httpClient.GetAsync(apiUrl + "GetbyId/" + idCorporacao);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);

                if (apiResponse?.value != null)
                {
                    return View("AdicionarDadosCorporacao", apiResponse.value);
                }
            }

            TempData["MensagemErro"] = "Erro ao carregar os dados da corporação.";
            return RedirectToAction("Index");
        }






        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(); 
            TempData["MensagemSucesso"] = "Logout realizado com sucesso!";
            return RedirectToAction("Index", "Home");
        }



        public class ApiResponse
        {
            public object result { get; set; }
            public CorporacaoModel value { get; set; }
        }


        public class TokenResponse
        {
            public string Token { get; set; }
            public int IdCorporacao { get; set; } 
        }

    }
}