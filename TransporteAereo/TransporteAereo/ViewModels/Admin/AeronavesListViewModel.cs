using System;
using System.ComponentModel.DataAnnotations;
using TransporteAereo.Models; 

namespace TransporteAereo.ViewModels.Admin
{
    public class AeronaveListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Tipo de Aeronave")]
        public TipoAeronave Tipo { get; set; }

        [Display(Name = "Modelo")]
        public string Modelo { get; set; } = string.Empty;
    }
}