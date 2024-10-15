using ChronosMVC.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ChronosMVC.Models.Enderecos
{
    public class LogradouroModel
    {
        [Key]
        public int idLogradouro { get; set; }
        public string cepLogradouro { get; set; } = string.Empty;
        public TipoLogradouro tipoLogradouro { get; set; }
        public string bairroLogradouro { get; set; } = string.Empty;
        public string cidadeLogradouro { get; set; } = string.Empty;
        public string ufLogradouro { get; set; } = string.Empty;
    }
}
