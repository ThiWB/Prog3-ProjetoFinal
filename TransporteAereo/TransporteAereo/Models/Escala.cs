using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransporteAereo.Models
{
    public class Escala
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Voo")]
        public Guid IdVoo { get; set; }

        [ForeignKey("AeroportoEscala")]
        public Guid IdAeroportoEscala { get; set; }

        public DateTime HorarioChegada { get; set; }

        public DateTime HorarioSaida { get; set; }

        public Voo Voo { get; set; } = null!;

        public Aeroporto AeroportoEscala { get; set; } = null!;
    }
}