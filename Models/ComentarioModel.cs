using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronosMVC.Models
{
    public class ComentarioModel
    {
        [Key]
        public int idComentario { get; set; }

        [ForeignKey("idPublicacao")]
        public int idPublicacao { get; set; }

        [ForeignKey("idEgresso")]
        public int idEgresso { get; set; }
        public string comentarioPublicacao { get; set; } = string.Empty;
    }
}
