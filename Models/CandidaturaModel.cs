using ChronosMVC.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronosMVC.Models
{
    public class CandidaturaModel
    {
        [Key]
        public int idCandidatura { get; set; }

        [ForeignKey("idEgresso")]
        public int idEgresso { get; set; }

        [ForeignKey("idVaga")]
        public int idVaga { get; set; }
        public DateTime dataIncricao { get; set; }


        public StatusCandidatura Status { get; set; } 
        public DateTime? DataAtualizacao { get; set; }
        public string? Notas { get; set; }
        public string? Feedback { get; set; }
    }
}
