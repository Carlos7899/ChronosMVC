using ChronosMVC.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronosMVC.Models
{
    public class EgressoModel
    {
        [Key]
        public int idEgresso { get; set; }
        public TipoEgresso tipoEgresso { get; set; }
        public string nomeEgresso { get; set; } = string.Empty;

        [EmailAddress]
        public string emailEgresso { get; set; } = string.Empty;
        public string numeroEgresso { get; set; } = string.Empty;
        public string cpfEgresso { get; set; } = string.Empty;

        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public DateTime? DataAcesso { get; set; }

        [MinLength(6)]
        [DataType(DataType.Password)]
        [NotMapped]
        public string PasswordString { get; set; } = string.Empty;

        [NotMapped]
        public string? Token { get; set; }
    }
}
