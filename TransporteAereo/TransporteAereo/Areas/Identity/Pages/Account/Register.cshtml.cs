#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using TransporteAereo.Models;
using System.Net;
using TransporteAereo.Controllers;
using TransporteAereo.ViewModels;

namespace TransporteAereo.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        //Inserção da nossa entidade ApplicationUser
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        //Instância de lista com os novos atributos do tipo Enum ao cadastrar novo usuário
        public List<SelectListItem> Generos { get; set; } = new();
        public List<SelectListItem> Tipos { get; set; } = new();

        //Método construtor
        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;

        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        //Entidades relacionadas a tela de Registro
        public class InputModel
        {
            [Required, StringLength(100)]
            public string Nome { get; set; } = null!;

            [Required, StringLength(100)]
            public string Sobrenome { get; set; } = null!;

            [Required]
            [StringLength(11, MinimumLength = 11)]
            public string CPF { get; set; } = null!;

            [Required]
            public int Idade { get; set; }

            [Required(ErrorMessage = "O campo CEP é obrigatório.")]
            [StringLength(8, MinimumLength = 8, ErrorMessage = "O CEP deve conter 8 dígitos.")]
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
            [StringLength(10)] 
            public string? Numero { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            public GeneroCliente Genero { get; set; }

            public TipoCliente Tipo { get; set; }

            public bool IsAdmin { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Tamanho de Senha Inválido", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "A Senha e Confirmação de Senha não se Equivalem")]
            public string ConfirmPassword { get; set; }
        }


        //Get para ter a população dos Enums e dos dados requisitados através do consumo da API
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            //Popular lista de Enums de Gênero
            Generos = Enum.GetValues(typeof(GeneroCliente))
                .Cast<GeneroCliente>()
                .Select(g => new SelectListItem
                {
                    Value = g.ToString(),
                    Text = g.ToString()
                })
                .ToList();

            //Popular lista de Enums de Tipo
            Tipos = Enum.GetValues(typeof(TipoCliente))
                .Cast<TipoCliente>()
                .Select(t => new SelectListItem
                {
                    Value = t.ToString(),
                    Text = t.ToString()
                })
                .ToList();

            var model = new CepViewModel();
        }

        //Post para criar o usuário
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                //Mapeamento dos campos persoanlizados da classe que estende o IdentityUser
                user.Nome = Input.Nome;
                user.Sobrenome = Input.Sobrenome;
                user.CPF = Input.CPF;
                user.Idade = Input.Idade;
                user.Genero = Input.Genero;
                user.Tipo = Input.Tipo;
                user.IsAdmin = Input.IsAdmin;

                user.Logradouro = Input.Logradouro;
                user.Bairro = Input.Bairro;
                user.Cidade = Input.Cidade;
                user.Estado = Input.Estado;
                user.Numero = Input.Numero;
                user.CodigoCEP = Input.CodigoCEP;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuário Criou nova Conta com Senha");

                    if(user.IsAdmin)
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }

                    else
                    {
                        await _userManager.AddToRoleAsync(user, "Usuario");
                    }


                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Por Favor, Confirme sua Conta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>CLICANDO AQUI</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        var userDataToDisplay = new
                        {
                            user.Nome,
                            user.Sobrenome,
                            user.Genero,
                            user.Idade,
                            user.IsAdmin,
                            GeneroUser = user.Genero.ToString(),
                            TipoUser = user.Tipo.ToString()
                        };

                        TempData["UserData"] = JsonSerializer.Serialize(userDataToDisplay);

                        return LocalRedirect(returnUrl);
                    }
                }

                //Popular lista de Enums de Gênero
                Generos = Enum.GetValues(typeof(GeneroCliente))
                    .Cast<GeneroCliente>()
                    .Select(g => new SelectListItem
                    {
                        Value = g.ToString(),
                        Text = g.ToString()
                    })
                    .ToList();

                //Popular lista de Enums de Tipo
                Tipos = Enum.GetValues(typeof(TipoCliente))
                    .Cast<TipoCliente>()
                    .Select(t => new SelectListItem
                    {
                        Value = t.ToString(),
                        Text = t.ToString()
                    })
                    .ToList();

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        //Método auxiliar que cria uma instância de ApplicationUser
        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        //Método auxiliar que agora faz o cast para IUserEmailStore<ApplicationUser>
        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}