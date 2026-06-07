using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TransporteAereo.Models;

namespace TransporteAereo.ViewModels.Admin
{
    public class AeronaveCompletaCreateViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O tipo da aeronave é obrigatório.")]
        [Display(Name = "Tipo de Aeronave")]
        public TipoAeronave Tipo { get; set; }

        [Required(ErrorMessage = "O modelo da aeronave é obrigatório.")]
        [StringLength(100)]
        [Display(Name = "Modelo")]
        public string Modelo { get; set; } = string.Empty;

        public List<AssentoCreateViewModel> Assentos { get; set; } = new List<AssentoCreateViewModel>();
    }
}
