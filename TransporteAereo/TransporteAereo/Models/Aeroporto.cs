using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransporteAereo.Models
{
    public class Aeroporto
    {
        [Key]
        public Guid Id { get; set; }

        [Required, StringLength(100)]
        public string NomeAeroporto { get; set; } = null!; 

        [Required, StringLength(100)]
        public string CidadeAeroporto { get; set; } = null!;

        [Required, StringLength(100)]
        public string EstadoAeroporto { get; set; } = null!;

        [Required, StringLength(100)]
        public string Pais { get; set; } = null!;


        public ICollection<Voo> VoosDeOrigem { get; set; } = new List<Voo>();

        public ICollection<Voo> VoosDeDestino { get; set; } = new List<Voo>();

        public ICollection<Escala> Escalas { get; set; } = new List<Escala>();

        public ICollection<Viagem> ViagensDeOrigem { get; set; } = new List<Viagem>();
        public ICollection<Viagem> ViagensDeDestino { get; set; } = new List<Viagem>();

    }
}