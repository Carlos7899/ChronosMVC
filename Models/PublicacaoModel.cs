using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronosMVC.Models
{
    public class PublicacaoModel
    {
        [Key]
        public int idPublicacao { get; set; }

        [ForeignKey("idCorporacao")]
        public int idCorporacao { get; set; }
        public string títuloPublicacao { get; set; } = string.Empty;
        public string conteudoPublicacao { get; set; } = string.Empty;
        public int avaliacaoPublicacao { get; set; }

        public DateTime dataCriacaoPublicacao { get; set; } = DateTime.UtcNow;
        public byte[]? imagemPublicacao { get; set; }
        public int? numeroVisualizacoes { get; set; }

        public List<ComentarioModel>? Comentarios { get; set; }
    }
}
