using System.ComponentModel.DataAnnotations;
using TransporteAereo.Models;

namespace TransporteAereo.ViewModels.Admin
{
    public class AssentoCreateViewModel
    {
        [Required(ErrorMessage = "O número do assento é obrigatório.")]
        [StringLength(5)]
        [Display(Name = "Número")]
        public string NumeroAssento { get; set; } = string.Empty;

        [Display(Name = "Classe")]
        public ClasseAssento Classe { get; set; }

        [Display(Name = "Localização")]
        public LocalizacaoPoltrona Localizacao { get; set; }

        [Display(Name = "Lado")]
        public LadoPoltrona Lado { get; set; }
    }
}
