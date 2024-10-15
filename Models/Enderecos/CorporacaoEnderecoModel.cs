using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChronosMVC.Models.Enderecos
{
    public class CorporacaoEnderecoModel
    {
        [Key]
        public int idCorporacaoEndereco { get; set; }

        [ForeignKey("idLogradouro")]
        public int idLogradouro { get; set; }

        [ForeignKey("idCorporacao")]
        public int idCorporacao { get; set; }
        public string numeroCorporacaoEndereco { get; set; } = string.Empty;
        public string complementoCorporacaoEndereco { get; set; } = string.Empty;

        // Revisar relacionamento, a corporação não pode aceitar um Logradouro que já pertence a alguem
        [NotMapped]
        [JsonIgnore]
        public CorporacaoModel? corporacao { get; set; }

        [NotMapped]
        [JsonIgnore]
        public LogradouroModel? logradouro { get; set; }
    }
}
