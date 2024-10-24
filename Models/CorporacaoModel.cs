using ChronosMVC.Models.Enderecos;
using ChronosMVC.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChronosMVC.Models
{
    public class CorporacaoModel
    {
        [Key]
        public int idCorporacao { get; set; }

        [Required(ErrorMessage = "O tipo da corporação é obrigatório.")]
        public TipoCorporacao tipoCorporacao { get; set; }
        [Required(ErrorMessage = "O nome da corporação é obrigatório.")]
        public string nomeCorporacao { get; set; } = string.Empty;

        [EmailAddress]
        public string emailCorporacao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O número da corporação é obrigatório.")]
        public string numeroCorporacao { get; set; } = string.Empty;
        public string descricaoCorporacao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CNPJ é obrigatório.")]
        [RegularExpression(@"^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$", ErrorMessage = "CNPJ inválido.")]
        public string cnpjCorporacao { get; set; } = string.Empty;


        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public DateTime? DataAcesso { get; set; }

        [NotMapped]
        public string PasswordString { get; set; } = string.Empty;

        [NotMapped]
        public string? Token { get; set; }



        [JsonIgnore]
        [NotMapped]
        public ICollection<VagaModel>? Vagas { get; set; }

        [JsonIgnore]
        [NotMapped]
        public ICollection<CursoModel> Cursos { get; set; }
    }
}
