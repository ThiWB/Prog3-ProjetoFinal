using System.ComponentModel.DataAnnotations;

namespace TransporteAereo.ViewModels.Admin
{
    public class EscalaInsertViewModel
    {
        [Required(ErrorMessage = "O aeroporto da escala é obrigatório.")]
        [Display(Name = "Aeroporto de Parada")]
        public Guid IdAeroportoEscala { get; set; }

        [Required(ErrorMessage = "O horário de chegada na escala é obrigatório.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Chegada na Parada")]
        public DateTime HorarioChegada { get; set; }

        [Required(ErrorMessage = "O horário de partida da escala é obrigatório.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Partida da Parada")]
        public DateTime HorarioSaida { get; set; }
    }
}
