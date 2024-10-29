using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronosMVC.Models
{
    public class CursoModel
    {
        [Key]
        public int idCurso { get; set; }

        [ForeignKey("idCorporacao")]
        public int idCorporacao { get; set; }

        [Required]
        [MaxLength(100)]
        public string nomeCurso { get; set; } = string.Empty;
        public string descricaoCurso { get; set; } = string.Empty;

        [Required]
        public int cargaHorariaCurso { get; set; } 

        [Required]
        [DataType(DataType.Date)]
        public DateTime dataInicioCurso { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime dataFimCurso { get; set; }
        public int? limiteVagas { get; set; } 




    }
}
