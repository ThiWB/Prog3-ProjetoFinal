using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TransporteAereo.Models;
using System.ComponentModel.DataAnnotations;

namespace TransporteAereo.Areas.Identity.Pages.Account
{

    // ViewModel para desserializar os dados
    public class UserDisplayViewModel
    {
        public string Nome { get; set; } = null!;
        public string Sobrenome { get; set; } = null!;
        public string CPF { get; set; } = null!;
        public int Idade { get; set; }
        public GeneroCliente Genero { get; set; } 
        public TipoCliente Tipo { get; set; }
        public string CodigoCEP { get; set; } = null!; 
        public string Logradouro { get; set; } 
        public string Numero { get; set; } 
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string Telefone { get; set; }
    }

    public class RegistrationSuccessModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationSuccessModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public UserDisplayViewModel? UserData { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToPage("/Index", new { area = "" });

            UserData = new UserDisplayViewModel
            {
                Nome = user.Nome,
                Sobrenome = user.Sobrenome,
                CPF = user.CPF,
                Idade = user.Idade,
                Genero = user.Genero,
                Tipo = user.Tipo,
                CodigoCEP = user.CodigoCEP,
                Logradouro = user.Logradouro,
                Numero = user.Numero,
                Bairro = user.Bairro,
                Cidade = user.Cidade,
                Estado = user.Estado,
                Telefone = user.PhoneNumber
            };

            return Page();
        }
    }
}