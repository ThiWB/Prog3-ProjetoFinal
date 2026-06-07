using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransporteAereo.Models
{
    public class Voo
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Aeronave")] 
        public Guid IdAeronave { get; set; }

        [ForeignKey("AeroportoOrigem")] 
        public Guid IdOrigem { get; set; }

        [ForeignKey("AeroportoDestino")] 
        public Guid IdDestino { get; set; }

        public DateTime HorarioSaida { get; set; }
        public DateTime HorarioChegada { get; set; }

        public decimal PrecoBase { get; set; }

        public Aeronave Aeronave { get; set; } = null!;

        [InverseProperty("VoosDeOrigem")]
        public Aeroporto AeroportoOrigem { get; set; } = null!;

        [InverseProperty("VoosDeDestino")]
        public Aeroporto AeroportoDestino { get; set; } = null!;

        public ICollection<Escala> Escalas { get; set; } = new List<Escala>();

        public ICollection<VooPoltrona> VooPoltronas { get; set; } = new List<VooPoltrona>();

        [ForeignKey("Viagem")]
        public Guid? IdViagem { get; set; }   

        public Viagem? Viagem { get; set; }   

    }
}