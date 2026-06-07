using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; 

namespace TransporteAereo.Models
{
    public class Aeronave
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O tipo da aeronave é obrigatório.")]
        public TipoAeronave Tipo { get; set; }

        [Required(ErrorMessage = "O modelo da aeronave é obrigatório.")]
        [StringLength(100, ErrorMessage = "O modelo deve ter no máximo 100 caracteres.")]
        public string Modelo { get; set; } = string.Empty;

        public ICollection<Assento> Assentos { get; set; } = new List<Assento>();

        public ICollection<Voo> Voos { get; set; } = new List<Voo>();
    }

    public enum TipoAeronave
    {
        NaoEspecificado = 0,
        AeronaveComercial,
        AeronaveExecutiva,
        AeronaveCargueira,
        AeronaveRegional,
        Jato
    }
}