using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ChronosMVC.Models.Curriculo
{
    public class FormacaoModel
    {
        [Key]
        public int idFormacao { get; set; }
        [ForeignKey("idCurriculo")]
        public int idCurriculo { get; set; }
        public string cursoFormacao { get; set; }
        public string instituicaoFormacao { get; set; }
        public DateTime dataConclusaoFormacao { get; set; }


        [NotMapped]
        [JsonIgnore]
        public CurriculoModel? Curriculo { get; set; }

    }
}
