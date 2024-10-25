namespace ChronosMVC.Models.Enderecos
{
    public class EnderecoViewModel
    {
        public LogradouroModel Logradouro { get; set; } = new LogradouroModel();
        public CorporacaoEnderecoModel CorporacaoEndereco { get; set; } = new CorporacaoEnderecoModel();
    }
}
