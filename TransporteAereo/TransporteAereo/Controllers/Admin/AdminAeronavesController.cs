using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TransporteAereo.Data;
using TransporteAereo.Models;
using TransporteAereo.ViewModels.Admin;

namespace TransporteAereo.Controllers.Admin
{
    public class AdminAeronavesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminAeronavesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var aeronaves = await _context.Aeronave
                .Select(a => new AeronaveListViewModel
                {
                    Id = a.Id,
                    Tipo = a.Tipo,
                    Modelo = a.Modelo
                }).ToListAsync();
            return View(aeronaves);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var aeronave = await _context.Aeronave
                .Include(a => a.Assentos)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aeronave == null) return NotFound();

            var viewModel = new AeronaveCompletaCreateViewModel
            {
                Id = aeronave.Id,
                Tipo = aeronave.Tipo,
                Modelo = aeronave.Modelo,
                Assentos = aeronave.Assentos.Select(a => new AssentoCreateViewModel
                {
                    NumeroAssento = a.NumeroAssento,
                    Classe = a.Classe,
                    Localizacao = a.Localizacao,
                    Lado = a.Lado
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View(new AeronaveCompletaCreateViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(AeronaveCompletaCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var aeronave = new Aeronave
            {
                Id = Guid.NewGuid(),
                Tipo = model.Tipo,
                Modelo = model.Modelo
            };
            _context.Aeronave.Add(aeronave);

            if (model.Assentos != null)
            {
                foreach (var assentoVm in model.Assentos)
                {
                    var assento = new Assento
                    {
                        Id = Guid.NewGuid(),
                        IdAeronave = aeronave.Id,
                        NumeroAssento = assentoVm.NumeroAssento,
                        Classe = assentoVm.Classe,
                        Localizacao = assentoVm.Localizacao,
                        Lado = assentoVm.Lado
                    };
                    _context.Assento.Add(assento);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var aeronave = await _context.Aeronave
                .Include(a => a.Assentos)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aeronave == null) return NotFound();

            var viewModel = new AeronaveCompletaCreateViewModel
            {
                Id = aeronave.Id,
                Tipo = aeronave.Tipo,
                Modelo = aeronave.Modelo,
                Assentos = aeronave.Assentos.Select(a => new AssentoCreateViewModel
                {
                    NumeroAssento = a.NumeroAssento,
                    Classe = a.Classe,
                    Localizacao = a.Localizacao,
                    Lado = a.Lado
                }).ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, AeronaveCompletaCreateViewModel model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var aeronave = await _context.Aeronave
                .Include(a => a.Assentos)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aeronave == null) return NotFound();

            aeronave.Tipo = model.Tipo;
            aeronave.Modelo = model.Modelo;

            var assentosParaRemover = aeronave.Assentos
                .Where(a => !model.Assentos.Any(vm => vm.NumeroAssento == a.NumeroAssento))
                .ToList();
            _context.Assento.RemoveRange(assentosParaRemover);

            foreach (var assentoVm in model.Assentos)
            {
                var assentoExistente = aeronave.Assentos
                    .FirstOrDefault(a => a.NumeroAssento == assentoVm.NumeroAssento);

                if (assentoExistente != null)
                {
                    assentoExistente.Classe = assentoVm.Classe;
                    assentoExistente.Localizacao = assentoVm.Localizacao;
                    assentoExistente.Lado = assentoVm.Lado;
                }
                else
                {
                    _context.Assento.Add(new Assento
                    {
                        Id = Guid.NewGuid(),
                        IdAeronave = aeronave.Id,
                        NumeroAssento = assentoVm.NumeroAssento,
                        Classe = assentoVm.Classe,
                        Localizacao = assentoVm.Localizacao,
                        Lado = assentoVm.Lado
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var aeronave = await _context.Aeronave
                .Include(a => a.Assentos)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aeronave == null) return NotFound();

            var viewModel = new AeronaveCompletaCreateViewModel
            {
                Id = aeronave.Id,
                Tipo = aeronave.Tipo,
                Modelo = aeronave.Modelo
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var aeronave = await _context.Aeronave
                .Include(a => a.Assentos)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aeronave != null)
            {
                _context.Assento.RemoveRange(aeronave.Assentos);
                _context.Aeronave.Remove(aeronave);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
