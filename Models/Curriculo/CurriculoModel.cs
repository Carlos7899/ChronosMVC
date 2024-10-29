using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChronosMVC.Models.Curriculo
{
    public class CurriculoModel
    {
        [Key]
        public int idCurriculo { get; set; }

        [ForeignKey("idEgresso")]
        public int idEgresso { get; set; }

        [EmailAddress]
        [MaxLength(150)]
        public string emailCurriculo { get; set; }
        public string telefoneCurriculo { get; set; }
        public string habilidadesCurriculo { get; set; }
        public string descricaoCurriculo { get; set; }


        [JsonIgnore]
        [NotMapped]
        public  EgressoModel? Egresso { get; set; }
        [JsonIgnore]
        [NotMapped]
        public List<ExperienciaModel>? ExperienciasProfissionais { get; set; }
        [JsonIgnore]
        [NotMapped]
        public List<FormacaoModel>? FormacoesAcademicas { get; set; }
    }
}
