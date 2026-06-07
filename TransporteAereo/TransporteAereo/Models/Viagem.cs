using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransporteAereo.Models
{
    public class Viagem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NomeViagem { get; set; } = string.Empty; 

        [Required]
        public Guid IdOrigem { get; set; }

        [Required]
        public Guid IdDestino { get; set; }

        public DateTime DataPartida { get; set; }

        public DateTime DataChegada { get; set; }

        public decimal PrecoTotal { get; set; }

        [ForeignKey("IdOrigem")]
        [InverseProperty("ViagensDeOrigem")]
        public Aeroporto AeroportoOrigem { get; set; } = null!;

        [ForeignKey("IdDestino")]
        [InverseProperty("ViagensDeDestino")]
        public Aeroporto AeroportoDestino { get; set; } = null!;

        [InverseProperty("Viagem")]
        public ICollection<Voo> Voos { get; set; } = new List<Voo>(); 
    }
}
