using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransporteAereo.Models
{
    public class Compra
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Cliente")]
        public Guid IdCliente { get; set; } 

        [ForeignKey("Viagem")]
        public Guid IdViagem { get; set; } 

        public DateTime DataCompra { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrecoTotal { get; set; }

        public StatusCompra Status { get; set; } = StatusCompra.Pendente;

        public ApplicationUser Cliente { get; set; } = null!;
        public Viagem Viagem { get; set; } = null!;

        [InverseProperty("Compra")]
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }

    public enum StatusCompra
    {
        Pendente, 
        Concluida,
        Cancelada
    }
}