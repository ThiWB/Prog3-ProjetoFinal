namespace TransporteAereo.Areas.Identity.Pages.Account.RestAPI
{
    public class CEP
    {
        public string? Codigo { get; set; }
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
    }

    public class ViaCEP
    {
        public string? cep { get; set; }
        public string? logradouro { get; set; }
        public string? complemento { get; set; }
        public string? unidade { get; set; }
        public string? bairro { get; set; }
        public string? localidade { get; set; }
        public string? uf { get; set; }
        public string? estado { get; set; }
        public string? regiao { get; set; }
        public string? ibge { get; set; }
        public string? gia { get; set; }
        public string? ddd { get; set; }
        public string? siafi { get; set; }

    }
}
