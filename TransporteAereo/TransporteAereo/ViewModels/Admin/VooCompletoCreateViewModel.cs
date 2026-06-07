using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using TransporteAereo.Models;

namespace TransporteAereo.ViewModels.Admin
{
    public class VooCompletoCreateViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "A aeronave é obrigatória.")]
        [Display(Name = "Aeronave")]
        public Guid IdAeronave { get; set; }

        [Required(ErrorMessage = "O aeroporto de origem é obrigatório.")]
        [Display(Name = "Origem")]
        public Guid IdOrigem { get; set; }

        [Required(ErrorMessage = "O aeroporto de destino é obrigatório.")]
        [Display(Name = "Destino")]
        public Guid IdDestino { get; set; }

        [Required(ErrorMessage = "O horário de partida é obrigatório.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Horário de Partida")]
        public DateTime HorarioSaida { get; set; }

        [Required(ErrorMessage = "O horário de chegada é obrigatório.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Horário de Chegada")]
        public DateTime HorarioChegada { get; set; }

        [Display(Name = "Preço Base do Trecho")]
        [DataType(DataType.Currency)]
        public decimal PrecoBase { get; set; }

        [Display(Name = "Escalas (Paradas)")]
        public List<EscalaInsertViewModel> Escalas { get; set; } = new List<EscalaInsertViewModel>();

        public IEnumerable<SelectListItem> AeronavesDisponiveis { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> AeroportosDisponiveis { get; set; } = new List<SelectListItem>();
    }
}