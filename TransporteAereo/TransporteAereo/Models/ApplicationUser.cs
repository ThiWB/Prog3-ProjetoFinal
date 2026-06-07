using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace TransporteAereo.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        [PersonalData]
        [Required, StringLength(100)]
        public string Nome { get; set; } = null!;

        [PersonalData]
        [Required, StringLength(100)]
        public string Sobrenome { get; set; } = null!;

        [PersonalData]
        [Required]
        [StringLength(11, MinimumLength = 11)]
        public string CPF { get; set; } = null!;

        [PersonalData]
        public int Idade { get; set; }

        [PersonalData]
        public GeneroCliente Genero { get; set; }

        [PersonalData]
        public TipoCliente Tipo { get; set; }

        public bool IsAdmin { get; set; }

        [PersonalData]
        [Required]
        [StringLength(8)]
        public string CodigoCEP { get; set; } = null!;

        [PersonalData]
        [StringLength(200)]
        public string? Logradouro { get; set; } 

        [PersonalData]
        [Required(ErrorMessage = "O número é obrigatório.")]
        [StringLength(10)]
        public string? Numero { get; set; } 

        [PersonalData]
        public string? Bairro { get; set; }

        [PersonalData]
        public string? Cidade { get; set; }

        [PersonalData]
        public string? Estado { get; set; }

        [PersonalData]
        [InverseProperty("Cliente")]
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    }

    public enum GeneroCliente
    {
        Masculino,
        Feminino
    }

    public enum TipoCliente
    {
        Preferencial,
        Normal
    }
}