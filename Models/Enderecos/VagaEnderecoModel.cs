using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChronosMVC.Models.Enderecos
{
    public class VagaEnderecoModel
    {
        [Key]
        public int idVagaEndereco { get; set; }

        [ForeignKey("idLogradouro")]
        public int idLogradouro { get; set; }

        [ForeignKey("idVaga")]
        public int idVaga { get; set; }
        public string numeroVagaEndereco { get; set; } = string.Empty;
        public string complementoVagaEndereco { get; set; } = string.Empty;

        [JsonIgnore]
        [NotMapped]
        public LogradouroModel? logradouro { get; set; }

        [JsonIgnore]
        [NotMapped]
        public VagaModel? vaga { get; set; }
    }
}
