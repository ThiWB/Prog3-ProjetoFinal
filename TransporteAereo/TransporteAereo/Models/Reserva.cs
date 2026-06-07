using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransporteAereo.Models
{
    public class Reserva
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Cliente")]
        public Guid IdCliente { get; set; }

        [ForeignKey("VooPoltrona")]
        public Guid IdVooPoltrona { get; set; }

        [ForeignKey("Compra")]
        public Guid IdCompra { get; set; }

        public DateTime DataReserva { get; set; } = DateTime.Now;

        public StatusReserva Status { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrecoReserva { get; set; } 

        public ApplicationUser Cliente { get; set; } = null!;

        public VooPoltrona VooPoltrona { get; set; } = null!;

        public Compra Compra { get; set; } = null!; 

    }

    public enum StatusReserva
    {
        Pendente,
        Confirmada,
        Cancelada
    }
}