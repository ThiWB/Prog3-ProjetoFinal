using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransporteAereo.Models
{
    public class VooPoltrona
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Voo")]
        public Guid IdVoo { get; set; }

        [ForeignKey("Assento")]
        public Guid IdPoltrona { get; set; }

        public StatusPoltrona Status { get; set; }

        public Voo Voo { get; set; } = null!;

        public Assento Assento { get; set; } = null!; 

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }

    public enum StatusPoltrona
    {
        Disponível,
        Reservado,
        Ocupado,
        Indisponível
    }
}
