using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TransporteAereo.Data;
using TransporteAereo.Models;
using QuestPDF.Fluent;
using TransporteAereo.PDF;
using TransporteAereo.Repository;

namespace TransporteAereo.Controllers
{
    public class ViagensController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISearchBarRepository _searchBarRepository;

        public ViagensController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ISearchBarRepository searchBarRepository)
        {
            _context = context;
            _userManager = userManager;
            _searchBarRepository = searchBarRepository;
        }

        private decimal CalcularAcrescimoPorClasse(Assento assento)
        {
            return assento.Classe switch
            {
                ClasseAssento.Executiva => 100.00m,
                ClasseAssento.Primeira => 200.00m,
                _ => 0.00m,
            };
        }

        private bool TryGetClienteId(out Guid idCliente)
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out idCliente))
            {
                idCliente = Guid.Empty;
                return false;
            }
            return true;
        }

 

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(string termoBusca)
        {
            var viagens = await _searchBarRepository.Search(termoBusca);

            ViewData["CurrentFilter"] = termoBusca;

            return View(viagens);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID da viagem não fornecido.";
                return NotFound();
            }

            var viagem = await _context.Viagem
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Voos)
                    .ThenInclude(voo => voo.AeroportoOrigem)
                .Include(v => v.Voos)
                    .ThenInclude(voo => voo.AeroportoDestino)
                .Include(v => v.Voos)
                    .ThenInclude(voo => voo.Escalas)            
                        .ThenInclude(escala => escala.AeroportoEscala)                                                                      
                .Include(v => v.Voos)
                    .ThenInclude(voo => voo.Aeronave)
                        .ThenInclude(a => a.Assentos)
                .Include(v => v.Voos)
                    .ThenInclude(voo => voo.VooPoltronas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (viagem == null)
            {
                TempData["ErrorMessage"] = "Viagem não encontrada.";
                return NotFound();
            }

            return View(viagem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Comprar([FromForm] List<Guid> assentoIds)
        {
            if (!TryGetClienteId(out Guid idCliente))
            {
                TempData["ErrorMessage"] = "Sua sessão expirou ou não está válida. Faça login para completar a compra.";
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            if (assentoIds == null || !assentoIds.Any())
            {
                TempData["ErrorMessage"] = "Nenhum assento foi selecionado. Escolha pelo menos um assento para continuar.";
                return RedirectToAction(nameof(Index));
            }

            var vooPoltronasReservar = await _context.VooPoltrona
                .Where(vp => assentoIds.Contains(vp.IdPoltrona))
                .Include(vp => vp.Voo)
                    .ThenInclude(v => v.Viagem)
                .Include(vp => vp.Assento)
                .ToListAsync();

            if (!vooPoltronasReservar.Any() || vooPoltronasReservar.Count != assentoIds.Count)
            {
                TempData["ErrorMessage"] = "Não foi possível confirmar a seleção de todos os assentos. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }

            var idViagem = vooPoltronasReservar.First().Voo.IdViagem;

            if (idViagem == null || vooPoltronasReservar.Any(vp => vp.Voo.IdViagem != idViagem))
            {
                TempData["ErrorMessage"] = "Os assentos selecionados devem pertencer à mesma Viagem.";
                return RedirectToAction(nameof(Index));
            }

            var viagem = vooPoltronasReservar.First().Voo.Viagem;

            decimal precoTotalCalculado = 0m;
            var reservasParaCriar = new List<Tuple<VooPoltrona, decimal>>();

            foreach (var vp in vooPoltronasReservar)
            {
                if (vp.Status != StatusPoltrona.Disponível)
                {
                    TempData["ErrorMessage"] = $"Conflito: O assento {vp.Assento.NumeroAssento} não está mais disponível.";
                    return RedirectToAction(nameof(Details), new { id = viagem.Id });
                }

                var precoReserva = viagem.PrecoTotal + CalcularAcrescimoPorClasse(vp.Assento);
                precoTotalCalculado += precoReserva;

                reservasParaCriar.Add(new Tuple<VooPoltrona, decimal>(vp, precoReserva));
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var novaCompra = new Compra
                    {
                        Id = Guid.NewGuid(),
                        IdViagem = idViagem.Value,
                        IdCliente = idCliente,
                        DataCompra = DateTime.Now,
                        PrecoTotal = precoTotalCalculado,
                        Status = StatusCompra.Pendente
                    };
                    _context.Compra.Add(novaCompra);

                    foreach (var (vooPoltrona, precoReserva) in reservasParaCriar)
                    {
                        vooPoltrona.Status = StatusPoltrona.Reservado;

                        var novaReserva = new Reserva
                        {
                            Id = Guid.NewGuid(),
                            IdCompra = novaCompra.Id,
                            IdCliente = idCliente,
                            IdVooPoltrona = vooPoltrona.Id,
                            DataReserva = DateTime.Now,
                            Status = StatusReserva.Pendente,
                            PrecoReserva = precoReserva
                        };
                        _context.Reserva.Add(novaReserva);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Assentos reservados com sucesso! Prossiga para a confirmação e pagamento.";
                    return RedirectToAction(nameof(ConfirmacaoCompra), new { id = novaCompra.Id });
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    TempData["ErrorMessage"] = "Ocorreu um erro inesperado ao processar sua reserva. Tente novamente mais tarde.";
                    return RedirectToAction(nameof(Details), new { id = idViagem });
                }
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ConfirmacaoCompra(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "ID da transação inválido.";
                return NotFound();
            }

            if (!TryGetClienteId(out Guid idCliente))
            {
                TempData["ErrorMessage"] = "Sessão inválida.";
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var compra = await _context.Compra
                .Include(c => c.Viagem).ThenInclude(v => v.AeroportoOrigem)
                .Include(c => c.Viagem).ThenInclude(v => v.AeroportoDestino)
                .Include(c => c.Reservas)
                    .ThenInclude(r => r.VooPoltrona)
                        .ThenInclude(vp => vp.Assento) 
                .Include(c => c.Reservas)
                    .ThenInclude(r => r.VooPoltrona)
                        .ThenInclude(vp => vp.Voo) 
                            .ThenInclude(v => v.Aeronave) 
                .Include(c => c.Reservas)
                    .ThenInclude(r => r.VooPoltrona)
                        .ThenInclude(vp => vp.Voo)
                            .ThenInclude(v => v.AeroportoOrigem) 
                .Include(c => c.Reservas)
                    .ThenInclude(r => r.VooPoltrona)
                        .ThenInclude(vp => vp.Voo)
                            .ThenInclude(v => v.AeroportoDestino) 
                .FirstOrDefaultAsync(c => c.Id == id && c.IdCliente == idCliente);

            if (compra == null)
            {
                TempData["ErrorMessage"] = "Transação de compra não encontrada ou você não tem permissão para acessá-la.";
                return NotFound();
            }

            return View(compra);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ProcessarPagamento(Guid idCompra)
        {
            if (!TryGetClienteId(out Guid idCliente))
            {
                TempData["ErrorMessage"] = "Sessão inválida.";
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var compra = await _context.Compra
                .Include(c => c.Reservas)
                    .ThenInclude(r => r.VooPoltrona)
                .Include(c => c.Viagem)
                    .ThenInclude(v => v.AeroportoOrigem)
                .Include(c => c.Viagem)
                    .ThenInclude(v => v.AeroportoDestino)
                .FirstOrDefaultAsync(c => c.Id == idCompra && c.IdCliente == idCliente);

            if (compra == null || compra.Status != StatusCompra.Pendente)
            {
                TempData["InfoMessage"] = "Transação inválida ou já processada.";
                return RedirectToAction(nameof(MinhasViagens));
            }

            bool pagamentoBemSucedido = true;

            if (pagamentoBemSucedido)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        compra.Status = StatusCompra.Concluida;
                        foreach (var reserva in compra.Reservas)
                        {
                            reserva.Status = StatusReserva.Confirmada;
                        }
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["SuccessMessage"] = "Pagamento efetuado com sucesso! Sua viagem está confirmada. Baixe seu recibo/passagem.";
                        return RedirectToAction(nameof(DownloadRecibo), new { id = compra.Id });
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        TempData["ErrorMessage"] = "Ocorreu um erro ao finalizar a compra e confirmar a reserva. Tente novamente.";
                        return RedirectToAction(nameof(ConfirmacaoCompra), new { id = compra.Id });
                    }
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Falha no processamento do pagamento. Tente novamente ou use outro método.";
                return RedirectToAction(nameof(ConfirmacaoCompra), new { id = compra.Id });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadRecibo(Guid id)
        {
            if (!TryGetClienteId(out Guid idCliente))
            {
                return Unauthorized();
            }

            var compra = await _context.Compra
                .Include(c => c.Viagem).ThenInclude(v => v.AeroportoOrigem)
                .Include(c => c.Viagem).ThenInclude(v => v.AeroportoDestino)
                .Include(c => c.Reservas)
                    .ThenInclude(r => r.VooPoltrona)
                        .ThenInclude(vp => vp.Assento) 
                .Include(c => c.Reservas)
                    .ThenInclude(r => r.VooPoltrona)
                        .ThenInclude(vp => vp.Voo) 
                            .ThenInclude(v => v.AeroportoOrigem)
                .Include(c => c.Reservas)
                    .ThenInclude(r => r.VooPoltrona)
                        .ThenInclude(vp => vp.Voo)
                            .ThenInclude(v => v.AeroportoDestino)
                .Include(c => c.Reservas)
                    .ThenInclude(r => r.VooPoltrona)
                        .ThenInclude(vp => vp.Voo)
                            .ThenInclude(v => v.Aeronave)
                .FirstOrDefaultAsync(c => c.Id == id && c.IdCliente == idCliente && c.Status == StatusCompra.Concluida);

            if (compra == null)
            {
                TempData["ErrorMessage"] = "Recibo/Passagem não encontrado ou transação não está confirmada.";
                return NotFound();
            }

            var document = new ReciboPdfDocument(compra);
            var pdfBytes = document.GeneratePdf();

            string fileName = $"Passagem_{compra.Viagem.NomeViagem.Replace(" ", "_")}_{compra.Id.ToString().Substring(0, 8)}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Recibo(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "ID da transação inválido.";
                return NotFound();
            }

            if (!TryGetClienteId(out Guid idCliente))
            {
                TempData["ErrorMessage"] = "Sessão inválida.";
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var compra = await _context.Compra
                .Include(c => c.Viagem).ThenInclude(v => v.AeroportoOrigem)
                .Include(c => c.Viagem).ThenInclude(v => v.AeroportoDestino)
                .Include(c => c.Reservas)
                    .ThenInclude(r => r.VooPoltrona)
                        .ThenInclude(vp => vp.Assento)
                .Include(c => c.Reservas)
                    .ThenInclude(r => r.VooPoltrona)
                        .ThenInclude(vp => vp.Voo) 
                            .ThenInclude(v => v.AeroportoOrigem)
                .Include(c => c.Reservas)
                    .ThenInclude(r => r.VooPoltrona)
                        .ThenInclude(vp => vp.Voo)
                            .ThenInclude(v => v.AeroportoDestino)
                .Include(c => c.Reservas)
                    .ThenInclude(r => r.VooPoltrona)
                        .ThenInclude(vp => vp.Voo)
                            .ThenInclude(v => v.Aeronave)
                .FirstOrDefaultAsync(c => c.Id == id && c.IdCliente == idCliente && c.Status == StatusCompra.Concluida);

            if (compra == null)
            {
                TempData["ErrorMessage"] = "Recibo não encontrado. A transação pode não estar confirmada.";
                return NotFound();
            }

            return View(compra);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MinhasViagens()
        {
            if (!TryGetClienteId(out Guid idCliente))
            {
                TempData["ErrorMessage"] = "Sessão inválida.";
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var minhasCompras = await _context.Compra
                .Where(c => c.IdCliente == idCliente && c.Status == StatusCompra.Concluida)
                .Include(c => c.Viagem)
                    .ThenInclude(v => v.AeroportoOrigem)
                .Include(c => c.Viagem)
                    .ThenInclude(v => v.AeroportoDestino)
                .OrderByDescending(c => c.DataCompra)
                .ToListAsync();

            return View(minhasCompras);
        }
    }
}