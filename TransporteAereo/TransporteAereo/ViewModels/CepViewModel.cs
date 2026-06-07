using System.ComponentModel.DataAnnotations;
using TransporteAereo.Areas.Identity.Pages.Account.RestAPI;

namespace TransporteAereo.ViewModels
{
    public class CepViewModel
    {
        [Required(ErrorMessage = "O campo CEP é obrigatório.")]
        [Display(Name = "CEP")]
        public string? CodigoCEP { get; set; }

        [Display(Name = "Logradouro")]
        public string? Logradouro { get; set; }

        [Display(Name = "Bairro")]
        public string? Bairro { get; set; }

        [Display(Name = "Cidade")]
        public string? Cidade { get; set; }

        [Display(Name = "Estado (UF)")]
        public string? Estado { get; set; }

        [Display(Name = "Número")]
        [Required(ErrorMessage = "O campo Número é obrigatório.")]
        public string? Numero { get; set; } 
    }
}
