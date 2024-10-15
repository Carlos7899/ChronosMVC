using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChronosMVC.Models.Enderecos
{
    public class EgressoEnderecoModel
    {
        [Key]
        public int idEgressoEndereco { get; set; }

        [ForeignKey("idLogradouro")]
        public int idLogradouro { get; set; }

        [ForeignKey("EgressoModel")]
        public int idEgresso { get; set; }
        public string numeroEgressoEndereco { get; set; } = string.Empty;
        public string complementoEgressoEndereco { get; set; } = string.Empty;

        [NotMapped]
        [JsonIgnore]
        public EgressoModel? Egresso { get; set; }

        [NotMapped]
        [JsonIgnore]
        public LogradouroModel? Logradouro { get; set; }


    }
}
