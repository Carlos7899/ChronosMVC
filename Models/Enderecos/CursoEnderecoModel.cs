using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChronosMVC.Models.Enderecos
{
    public class CursoEnderecoModel
    {
        [Key]
        public int idCursoEndereco { get; set; }

        [ForeignKey("idCurso")]
        public int idCurso { get; set; }

        [ForeignKey("idLogradouro")]
        public int idLogradouro { get; set; }

        public string numeroCursoEndereco { get; set; } = string.Empty;
        public string complementoCursoEndereco { get; set; } = string.Empty;
       

        [JsonIgnore]
        [NotMapped]
        public LogradouroModel? logradouro { get; set; }

        [JsonIgnore]
        [NotMapped]
        public CursoModel? curso { get; set; }
    }
}
