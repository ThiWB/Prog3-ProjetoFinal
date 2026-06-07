using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransporteAereo.Models
{
    public class Assento
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Aeronave")] 
        public Guid IdAeronave { get; set; }

        [Required]
        [StringLength(5)]
        public string? NumeroAssento { get; set; } 

        public ClasseAssento Classe {  get; set; }

        public LocalizacaoPoltrona Localizacao { get; set; } 

        public LadoPoltrona Lado { get; set; } 

        public Aeronave Aeronave { get; set; } = null!;

        public ICollection<VooPoltrona> VooPoltronas { get; set; } = new List<VooPoltrona>();
    }

    public enum ClasseAssento
    {
        Economica,
        Executiva,
        Primeira
    }

    public enum LocalizacaoPoltrona 
    {
        Janela,
        Corredor,
        Meio
    }

    public enum LadoPoltrona 
    {
        Esquerda,
        Meio,
        Direita
    }
}
