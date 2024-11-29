using ChronosMVC.Models;
using ChronosMVC.Models.Curriculo;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChronosMVC.Controllers
{
    public class CurriculoController : Controller
    {
        private readonly string apiUrlLocal = "http://localhost:5027/api/Curriculo/";
        private readonly string apiExperienciaUrlLocal = "http://localhost:5027/api/Experiencia/";
        private readonly string apiFormacaoUrlLocal = "http://localhost:5027/api/Formacao/";


        private readonly string apiUrl = "http://Chronos.somee.com/ChronosApi/api/Curriculo/";
        private readonly string apiExperienciaUrl = "http://Chronos.somee.com/ChronosApi/api/Experiencia/";
        private readonly string apiFormacaoUrl = "http://Chronos.somee.com/ChronosApi/api/Formacao/";

        private readonly HttpClient _httpClient;

        public CurriculoController()
        {
            _httpClient = new HttpClient();
        }

        //certo
        #region Ação para buscar ou criar o currículo do egresso logado

        [HttpGet]
        public async Task<IActionResult> Detalhes()
        {
            try
            {
                // Obtendo o ID do Egresso logado
                var idEgresso = GetLoggedEgressoId();

                // Carregar o currículo do egresso
                var apiCurriculoEndpoint = apiUrl + $"GetByEgresso/{idEgresso}";
                HttpResponseMessage responseCurriculo = await _httpClient.GetAsync(apiCurriculoEndpoint);

                if (responseCurriculo.IsSuccessStatusCode)
                {
                    var jsonCurriculo = await responseCurriculo.Content.ReadAsStringAsync();
                    var curriculo = JsonConvert.DeserializeObject<CurriculoModel>(jsonCurriculo);

                    if (curriculo != null)
                    {
                        // Carregar as experiências profissionais
                        var apiExperienciaEndpoint = apiExperienciaUrl + $"GetByCurriculo/{curriculo.idCurriculo}";
                        HttpResponseMessage responseExperiencia = await _httpClient.GetAsync(apiExperienciaEndpoint);

                        if (responseExperiencia.IsSuccessStatusCode)
                        {
                            var jsonExperiencia = await responseExperiencia.Content.ReadAsStringAsync();
                            curriculo.ExperienciasProfissionais = JsonConvert.DeserializeObject<List<ExperienciaModel>>(jsonExperiencia);
                        }

                        // Carregar as formações acadêmicas
                        var apiFormacaoEndpoint = apiFormacaoUrl + $"GetByCurriculo/{curriculo.idCurriculo}";
                        HttpResponseMessage responseFormacao = await _httpClient.GetAsync(apiFormacaoEndpoint);

                        if (responseFormacao.IsSuccessStatusCode)
                        {
                            var jsonFormacao = await responseFormacao.Content.ReadAsStringAsync();
                            curriculo.FormacoesAcademicas = JsonConvert.DeserializeObject<List<FormacaoModel>>(jsonFormacao);
                        }

                        // **Carregar o nome do egresso**
                        ViewBag.nomeEgresso = await GetNomeEgresso(idEgresso);

                        return View(curriculo); // Retorna a view com o currículo carregado
                    }
                }
                else
                {
                    string errorResponse = await responseCurriculo.Content.ReadAsStringAsync();
                    TempData["MensagemErro"] = "Erro ao carregar currículo: " + errorResponse;
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro ao carregar currículo: " + ex.Message;
            }

            return View(new CurriculoModel()); // Retorna uma view vazia caso não consiga carregar o currículo
        }


        #endregion

        //certo
        #region Ação para criar o currículo (POST)
        public IActionResult CriarCurriculo()
        {
            var curriculoModel = new CurriculoModel(); // Garantir que o modelo está inicializado
            return View(curriculoModel); // Passa o modelo para a view
        }


        // Criar o currículo e passar para a próxima etapa (etapa 2)
        [HttpPost]
        public async Task<IActionResult> CriarCurriculo(CurriculoModel curriculo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var idEgresso = GetLoggedEgressoId(); // Método fictício para pegar o ID do egresso logado
                    curriculo.idEgresso = idEgresso;

                    // Enviar para a API para criar o currículo
                    string serializedCurriculo = JsonConvert.SerializeObject(curriculo);
                    var content = new StringContent(serializedCurriculo, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await _httpClient.PostAsync(apiUrl + "POST", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["MensagemSucesso"] = "Currículo criado com sucesso!";
                        return RedirectToAction("CriarExperiencia"); 
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        throw new Exception(errorResponse);
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = ex.Message;
                }
            }

            // Se não for válido, retorna a view para corrigir
            return View(curriculo);
        }
        #endregion

        //certo
        #region Ação para edição do currículo
        [HttpGet]
        public async Task<IActionResult> EditarCurriculo()
        {
            try
            {
                var idCurriculo = await GetLoggedCurriculoIdAsync(); // Obtém o idCurriculo do usuário logado

                // Chama a API para buscar os dados do currículo
                var apiCurriculoEndpoint = apiUrl + $"GetById/{idCurriculo}";
                HttpResponseMessage response = await _httpClient.GetAsync(apiCurriculoEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    var jsonCurriculo = await response.Content.ReadAsStringAsync();
                    var curriculo = JsonConvert.DeserializeObject<CurriculoModel>(jsonCurriculo);
                    return View(curriculo); // Retorna a view com os dados do currículo para edição
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["MensagemErro"] = "Erro ao carregar os dados do currículo: " + errorResponse;
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro ao carregar os dados do currículo: " + ex.Message;
            }

            return View(new CurriculoModel());
        }

        [HttpPost]
        public async Task<IActionResult> EditarCurriculo(CurriculoModel curriculo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Obtém o ID do Currículo do egresso logado
                    var idCurriculo = await GetLoggedCurriculoIdAsync();

                    // Associa o idCurriculo ao objeto do modelo
                    curriculo.idCurriculo = idCurriculo;

                    // Serializa o objeto do currículo para enviar via API
                    string serializedCurriculo = JsonConvert.SerializeObject(curriculo);
                    var content = new StringContent(serializedCurriculo, Encoding.UTF8, "application/json");

                    // Envia os dados para a API para atualizar o currículo
                    HttpResponseMessage response = await _httpClient.PutAsync(apiUrl + $"Put/{curriculo.idCurriculo}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["MensagemSucesso"] = "Currículo atualizado com sucesso!";
                        return RedirectToAction("Detalhes"); // Redireciona para a página de detalhes do currículo
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        TempData["MensagemErro"] = "Erro ao atualizar o currículo: " + errorResponse;
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = "Erro ao atualizar o currículo: " + ex.Message;
                }
            }

            return View(curriculo); // Se houver erros de validação, retorna ao formulário de edição
        }
        #endregion

        //certo
        #region Ação para editar o currículo
        [HttpGet]
        public async Task<IActionResult> Editar()
        {
            try
            {
                var idEgresso = GetLoggedEgressoId(); // Pegue o ID do egresso logado
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl + $"GetByEgresso/{idEgresso}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var curriculo = JsonConvert.DeserializeObject<CurriculoModel>(jsonResponse);
                    return View(curriculo);
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception(errorResponse);
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("Detalhes");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CurriculoModel curriculo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Serializa o objeto do currículo
                    string serializedCurriculo = JsonConvert.SerializeObject(curriculo);
                    var content = new StringContent(serializedCurriculo, Encoding.UTF8, "application/json");

                    // Envia a requisição PUT para atualizar o currículo
                    HttpResponseMessage response = await _httpClient.PutAsync(apiUrl + $"Put/{curriculo.idCurriculo}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["MensagemSucesso"] = "Currículo atualizado com sucesso!";
                        return RedirectToAction("Detalhes");
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        throw new Exception(errorResponse);
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = ex.Message;
                }
            }
            // Se o modelo não for válido, retorna à view com o modelo
            return View(curriculo);
        }
        #endregion

        //certo
        #region  Ação para criar uma nova experiência
        [HttpGet]
        public IActionResult CriarExperiencia()
        {
            var experiencia = new ExperienciaModel(); 
            return View(experiencia); 
        }

        [HttpPost]
        public async Task<IActionResult> CriarExperiencia(ExperienciaModel experiencia)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Chama o método assíncrono para obter o idCurriculo
                    var idCurriculo = await GetLoggedCurriculoIdAsync();
                    experiencia.idCurriculo = idCurriculo;  // Associa o currículo à experiência

                    // Envia a experiência para a API
                    string serializedExperiencia = JsonConvert.SerializeObject(experiencia);
                    var content = new StringContent(serializedExperiencia, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PostAsync(apiExperienciaUrl + "POST", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["MensagemSucesso"] = "Experiência adicionada com sucesso!";
                        return RedirectToAction("CriarFormacao"); // Redireciona para a criação de formação acadêmica
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        TempData["MensagemErro"] = "Erro ao adicionar experiência: " + errorResponse;
                        return View(experiencia); // Retorna para o formulário com a mensagem de erro
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = "Erro ao adicionar experiência: " + ex.Message;
                }
            }
            return View(experiencia);
        }

        #endregion

        //certo
        #region Ação para criar uma nova formação acadêmica

        [HttpGet]
        public IActionResult CriarFormacao()
        {
            var formacao = new FormacaoModel();
            return View(formacao);
        }

        [HttpPost]
        public async Task<IActionResult> CriarFormacao(FormacaoModel formacao)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Chama o método assíncrono para obter o idCurriculo
                    var idCurriculo = await GetLoggedCurriculoIdAsync();
                    formacao.idCurriculo = idCurriculo;  // Associa o currículo à formação

                    // Envia a formação para a API
                    string serializedFormacao = JsonConvert.SerializeObject(formacao);
                    var content = new StringContent(serializedFormacao, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PostAsync(apiFormacaoUrl + "POST", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["MensagemSucesso"] = "Formação adicionada com sucesso!";
                        return RedirectToAction("Detalhes"); // Redireciona para a página de detalhes do currículo
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        TempData["MensagemErro"] = "Erro ao adicionar formação: " + errorResponse;
                        return View(formacao); // Retorna para o formulário com a mensagem de erro
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = "Erro ao adicionar formação: " + ex.Message;
                }
            }
            return View(formacao);
        }

        #endregion

        //certo
        #region experiencia

        //certo
        #region ação para buscar as experiencias do egresso logado
        [HttpGet]
        public async Task<IActionResult> ListaExperiencias()
        {
            try
            {
                var idCurriculo = await GetLoggedCurriculoIdAsync(); // Obtém o idCurriculo

                // Chama a API para obter as experiências do currículo
                var apiExperienciaEndpoint = apiExperienciaUrl + $"GetByCurriculo/{idCurriculo}";
                HttpResponseMessage responseExperiencia = await _httpClient.GetAsync(apiExperienciaEndpoint);

                if (responseExperiencia.IsSuccessStatusCode)
                {
                    var jsonExperiencia = await responseExperiencia.Content.ReadAsStringAsync();
                    var experiencias = JsonConvert.DeserializeObject<List<ExperienciaModel>>(jsonExperiencia);

                    return View(experiencias); // Retorna a lista de experiências para a view
                }
                else
                {
                    string errorResponse = await responseExperiencia.Content.ReadAsStringAsync();
                    TempData["MensagemErro"] = "Erro ao carregar experiências: " + errorResponse;
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro ao carregar experiências: " + ex.Message;
            }

            return View(new List<ExperienciaModel>());
        }
        #endregion

        //certo
        #region ação para editar a experiencia 
        // Ação para carregar a experiência que será editada
        [HttpGet]
        public async Task<IActionResult> EditarExperiencia(int idExperiencia)
        {
            try
            {
                var response = await _httpClient.GetAsync(apiExperienciaUrl + $"GetById/{idExperiencia}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonExperiencia = await response.Content.ReadAsStringAsync();
                    var experiencia = JsonConvert.DeserializeObject<ExperienciaModel>(jsonExperiencia);
                    return View(experiencia); // Retorna a experiência para edição
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["MensagemErro"] = "Erro ao carregar experiência: " + errorResponse;
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro ao carregar experiência: " + ex.Message;
            }

            return View(new ExperienciaModel());
        }

        // Ação POST para salvar a experiência editada
        [HttpPost]
        public async Task<IActionResult> EditarExperiencia(ExperienciaModel experiencia)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string serializedExperiencia = JsonConvert.SerializeObject(experiencia);
                    var content = new StringContent(serializedExperiencia, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PutAsync(apiExperienciaUrl + $"Put/{experiencia.idExperiencia}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["MensagemSucesso"] = "Experiência atualizada com sucesso!";
                        return RedirectToAction("ListaExperiencias");
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        TempData["MensagemErro"] = "Erro ao atualizar experiência: " + errorResponse;
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = "Erro ao atualizar experiência: " + ex.Message;
                }
            }
            return View(experiencia); // Retorna à view para corrigir erros
        }

        #endregion

        //certo
        #region ação para excluir a experiencia 
        // Ação para excluir uma experiência
        [HttpPost]
        public async Task<IActionResult> ExcluirExperiencia(int idExperiencia)
        {
            try
            {
                // Envia a requisição DELETE para a API
                HttpResponseMessage response = await _httpClient.DeleteAsync(apiExperienciaUrl + $"Delete/{idExperiencia}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["MensagemSucesso"] = "Experiência excluída com sucesso!";
                    return RedirectToAction("ListaExperiencias"); // Redireciona para a lista de experiências
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["MensagemErro"] = "Erro ao excluir experiência: " + errorResponse;
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro ao excluir experiência: " + ex.Message;
            }

            return RedirectToAction("ListaExperiencias"); // Se houver erro, redireciona para a lista
        }

        #endregion

        //certo
        #region ação para adicionar uma nova experiencia no curiculo
        [HttpGet]
        public IActionResult AdicionarExperiencia()
        {
            var experiencia = new ExperienciaModel();
            return View(experiencia);
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarExperiencia(ExperienciaModel experiencia)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var idCurriculo = await GetLoggedCurriculoIdAsync(); 
                    experiencia.idCurriculo = idCurriculo;  

                    // Envia a experiência para a API
                    string serializedExperiencia = JsonConvert.SerializeObject(experiencia);
                    var content = new StringContent(serializedExperiencia, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PostAsync(apiExperienciaUrl + "POST", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["MensagemSucesso"] = "Experiência adicionada com sucesso!";
                        return RedirectToAction("ListaExperiencias"); // Redireciona para a lista de experiências
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        TempData["MensagemErro"] = "Erro ao adicionar experiência: " + errorResponse;
                        return View(experiencia); // Retorna para o formulário com a mensagem de erro
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = "Erro ao adicionar experiência: " + ex.Message;
                }
            }
            return View(experiencia);
        }

        #endregion

        #endregion

        //certo
        #region Formações

        //certo
        #region ação para buscar as formações do egresso logado
        [HttpGet]
        public async Task<IActionResult> ListaFormacoes()
        {
            try
            {
                var idCurriculo = await GetLoggedCurriculoIdAsync(); // Obtém o idCurriculo

                // Chama a API para obter as formações do currículo
                var apiFormacaoEndpoint = apiFormacaoUrl + $"GetByCurriculo/{idCurriculo}";
                HttpResponseMessage responseFormacao = await _httpClient.GetAsync(apiFormacaoEndpoint);

                if (responseFormacao.IsSuccessStatusCode)
                {
                    var jsonFormacao = await responseFormacao.Content.ReadAsStringAsync();
                    var formacoes = JsonConvert.DeserializeObject<List<FormacaoModel>>(jsonFormacao);

                    return View(formacoes); // Retorna a lista de formações para a view
                }
                else
                {
                    string errorResponse = await responseFormacao.Content.ReadAsStringAsync();
                    TempData["MensagemErro"] = "Erro ao carregar formações: " + errorResponse;
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro ao carregar formações: " + ex.Message;
            }

            return View(new List<FormacaoModel>());
        }
        #endregion

        //certo
        #region ação para editar a formação
        [HttpGet]
        public async Task<IActionResult> EditarFormacao(int idFormacao)
        {
            try
            {
                var response = await _httpClient.GetAsync(apiFormacaoUrl + $"GetById/{idFormacao}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonFormacao = await response.Content.ReadAsStringAsync();
                    var formacao = JsonConvert.DeserializeObject<FormacaoModel>(jsonFormacao);
                    return View(formacao); // Retorna a formação para edição
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["MensagemErro"] = "Erro ao carregar formação: " + errorResponse;
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro ao carregar formação: " + ex.Message;
            }

            return View(new FormacaoModel());
        }


        [HttpPost]
        public async Task<IActionResult> EditarFormacao(FormacaoModel formacao)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string serializedFormacao = JsonConvert.SerializeObject(formacao);
                    var content = new StringContent(serializedFormacao, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PutAsync(apiFormacaoUrl + $"Put/{formacao.idFormacao}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["MensagemSucesso"] = "Formação atualizada com sucesso!";
                        return RedirectToAction("ListaFormacoes");
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        TempData["MensagemErro"] = "Erro ao atualizar formação: " + errorResponse;
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = "Erro ao atualizar formação: " + ex.Message;
                }
            }
            return View(formacao); // Retorna à view para corrigir erros
        }

        #endregion

        //certo
        #region ação para excluir a formação
        [HttpPost]
        public async Task<IActionResult> ExcluirFormacao(int idFormacao)
        {
            try
            {
                // Envia a requisição DELETE para a API
                HttpResponseMessage response = await _httpClient.DeleteAsync(apiFormacaoUrl + $"Delete/{idFormacao}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["MensagemSucesso"] = "Formação excluída com sucesso!";
                    return RedirectToAction("ListaFormacoes"); // Redireciona para a lista de formações
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["MensagemErro"] = "Erro ao excluir formação: " + errorResponse;
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro ao excluir formação: " + ex.Message;
            }

            return RedirectToAction("ListaFormacoes"); // Se houver erro, redireciona para a lista
        }
        #endregion

        //certo
        #region ação para adicionar uma nova formação no currículo
        [HttpGet]
        public IActionResult AdicionarFormacao()
        {
            var formacao = new FormacaoModel();
            return View(formacao);
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarFormacao(FormacaoModel formacao)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var idCurriculo = await GetLoggedCurriculoIdAsync();
                    formacao.idCurriculo = idCurriculo;

                    // Envia a formação para a API
                    string serializedFormacao = JsonConvert.SerializeObject(formacao);
                    var content = new StringContent(serializedFormacao, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PostAsync(apiFormacaoUrl + "POST", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["MensagemSucesso"] = "Formação adicionada com sucesso!";
                        return RedirectToAction("ListaFormacoes"); // Redireciona para a lista de formações
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        TempData["MensagemErro"] = "Erro ao adicionar formação: " + errorResponse;
                        return View(formacao); // Retorna para o formulário com a mensagem de erro
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = "Erro ao adicionar formação: " + ex.Message;
                }
            }
            return View(formacao);
        }
        #endregion

        #endregion

        //certo
        #region metodos auxiliares
        private async Task<int> GetLoggedCurriculoIdAsync()
        {
            try
            {
                // Obtém o ID do egresso logado
                var idEgresso = GetLoggedEgressoId();

                // Chama a API para buscar o currículo do egresso
                var apiEndpoint = apiUrl + $"GetByEgresso/{idEgresso}";
                HttpResponseMessage response = await _httpClient.GetAsync(apiEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var curriculo = JsonConvert.DeserializeObject<CurriculoModel>(jsonResponse);

                    // Se o currículo for encontrado, retorna o idCurriculo
                    if (curriculo != null)
                    {
                        return curriculo.idCurriculo;
                    }
                    else
                    {
                        throw new Exception("Currículo não encontrado para o egresso.");
                    }
                }
                else
                {
                    throw new Exception($"Erro ao buscar currículo: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao obter o ID do currículo: " + ex.Message);
            }
        }



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

        private async Task<string> GetNomeEgresso(int idEgresso)
        {
            try
            {
                // Endpoint da API para buscar o egresso pelo ID
                var apiEgressoEndpoint = $"http://Chronos.somee.com/ChronosApi/api/Egresso/GetbyId/{idEgresso}";

                // Faz a requisição para a API
                HttpResponseMessage response = await _httpClient.GetAsync(apiEgressoEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // Deserializa a resposta JSON para o tipo ApiResponse
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);

                    // Verifica se a resposta possui um egresso válido e retorna o nome
                    if (apiResponse != null && apiResponse.value != null)
                    {
                        return apiResponse.value.nomeEgresso ?? "Nome não encontrado";
                    }
                    else
                    {
                        return "Nome não encontrado";
                    }
                }
                else
                {
                    throw new Exception($"Erro ao buscar nome do egresso: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna uma mensagem mais detalhada
                return $"Erro ao obter nome do egresso: {ex.Message}";
            }
        }



        public class EgressoResponse
        {
            public string nomeEgresso { get; set; }
         
        }

        public class ApiResponse
        {
            public object result { get; set; }
            public EgressoModel value { get; set; }
        }

        #endregion
    }
}
