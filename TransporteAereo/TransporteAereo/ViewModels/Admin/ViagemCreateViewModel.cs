using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace TransporteAereo.ViewModels.Admin
{
    public class ViagemCreateViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O nome da viagem é obrigatório.")]
        [StringLength(100)]
        [Display(Name = "Nome da Viagem")]
        public string NomeViagem { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione pelo menos um voo para a viagem.")]
        [Display(Name = "Voos Selecionados (em Ordem)")]
        public List<Guid> IdsVoosSelecionados { get; set; } = new List<Guid>();

        public IEnumerable<SelectListItem> VoosDisponiveis { get; set; } = new List<SelectListItem>();
    }

    public class ViagemEditViewModel : ViagemCreateViewModel
    {
        public Guid Id { get; set; }
    }
}
