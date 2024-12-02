using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChronosMVC.Models.Curriculo
{
    public class ExperienciaModel
    {
        [Key]
        public int idExperiencia { get; set; }
        [ForeignKey("idCurriculo")]
        public int idCurriculo { get; set; }
        public string cargoExperiencia { get; set; }
        public string empresaExperiencia { get; set; }
        public DateTime dataInicioExperiencia { get; set; } = DateTime.Today;
        public DateTime? dataFimExperiencia { get; set; }
        public string Descricao { get; set; }


        [NotMapped]
        [JsonIgnore]
        public CurriculoModel? Curriculo { get; set; }
    }
}
