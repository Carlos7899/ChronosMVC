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

    }
}
