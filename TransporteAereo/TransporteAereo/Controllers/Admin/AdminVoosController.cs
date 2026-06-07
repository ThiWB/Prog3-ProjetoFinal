using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransporteAereo.Data;
using TransporteAereo.Models;
using TransporteAereo.ViewModels.Admin;

namespace TransporteAereo.Controllers.Admin
{
    public class AdminVoosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminVoosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var voos = _context.Voo
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave);

            return View(await voos.ToListAsync());
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voo = await _context.Voo
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .Include(v => v.Escalas)
                    .ThenInclude(e => e.AeroportoEscala)
                .Include(v => v.VooPoltronas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (voo == null)
            {
                return NotFound();
            }

            return View(voo);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new VooCompletoCreateViewModel
            {
                AeronavesDisponiveis = await GetAeronavesDropdown(),
                AeroportosDisponiveis = await GetAeroportosDropdown(),
                Escalas = new List<EscalaInsertViewModel>()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(VooCompletoCreateViewModel model)
        {
            if (model.HorarioSaida >= model.HorarioChegada)
            {
                ModelState.AddModelError(string.Empty, "O horário de partida deve ser anterior ao horário de chegada.");
            }
            if (model.IdOrigem == model.IdDestino)
            {
                ModelState.AddModelError(string.Empty, "O Aeroporto de Origem e Destino não podem ser os mesmos.");
            }

            if (ModelState.IsValid)
            {
                var voo = new Voo
                {
                    Id = Guid.NewGuid(),
                    IdAeronave = model.IdAeronave,
                    IdOrigem = model.IdOrigem,
                    IdDestino = model.IdDestino,
                    HorarioSaida = model.HorarioSaida,
                    HorarioChegada = model.HorarioChegada,
                    PrecoBase = model.PrecoBase
                };

                _context.Voo.Add(voo);
                await _context.SaveChangesAsync();

                if (model.Escalas != null)
                {
                    foreach (var escalaVm in model.Escalas)
                    {
                        var escala = new Escala
                        {
                            Id = Guid.NewGuid(),
                            IdVoo = voo.Id, 
                            IdAeroportoEscala = escalaVm.IdAeroportoEscala,
                            HorarioChegada = escalaVm.HorarioChegada,
                            HorarioSaida = escalaVm.HorarioSaida
                        };
                        _context.Escala.Add(escala);
                    }
                }

                await GerarVooPoltronas(voo.Id, voo.IdAeronave);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            model.AeronavesDisponiveis = await GetAeronavesDropdown();
            model.AeroportosDisponiveis = await GetAeroportosDropdown();
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voo = await _context.Voo
                .Include(v => v.Escalas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (voo == null)
            {
                return NotFound();
            }

            var model = new VooCompletoCreateViewModel
            {
                IdAeronave = voo.IdAeronave,
                IdOrigem = voo.IdOrigem,
                IdDestino = voo.IdDestino,
                HorarioSaida = voo.HorarioSaida,
                HorarioChegada = voo.HorarioChegada,
                PrecoBase = voo.PrecoBase,

                AeronavesDisponiveis = await GetAeronavesDropdown(),
                AeroportosDisponiveis = await GetAeroportosDropdown(),

                Escalas = voo.Escalas.Select(e => new EscalaInsertViewModel
                {
                    IdAeroportoEscala = e.IdAeroportoEscala,
                    HorarioChegada = e.HorarioChegada,
                    HorarioSaida = e.HorarioSaida
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, VooCompletoCreateViewModel model)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            if (model.HorarioSaida >= model.HorarioChegada)
            {
                ModelState.AddModelError(string.Empty, "O horário de partida deve ser anterior ao horário de chegada.");
            }
            if (model.IdOrigem == model.IdDestino)
            {
                ModelState.AddModelError(string.Empty, "O Aeroporto de Origem e Destino não podem ser os mesmos.");
            }

            if (ModelState.IsValid)
            {
                var voo = await _context.Voo.FindAsync(id);
                if (voo == null) return NotFound();

                try
                {
                    voo.IdAeronave = model.IdAeronave;
                    voo.IdOrigem = model.IdOrigem;
                    voo.IdDestino = model.IdDestino;
                    voo.HorarioSaida = model.HorarioSaida;
                    voo.HorarioChegada = model.HorarioChegada;
                    voo.PrecoBase = model.PrecoBase;

                    _context.Update(voo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VooExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            model.AeronavesDisponiveis = await GetAeronavesDropdown();
            model.AeroportosDisponiveis = await GetAeroportosDropdown();
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voo = await _context.Voo
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (voo == null)
            {
                return NotFound();
            }

            return View(voo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var voo = await _context.Voo
                .Include(v => v.Escalas)
                .Include(v => v.VooPoltronas)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (voo != null)
            {
                _context.Escala.RemoveRange(voo.Escalas);
                _context.VooPoltrona.RemoveRange(voo.VooPoltronas);
                _context.Voo.Remove(voo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private async Task<IEnumerable<SelectListItem>> GetAeronavesDropdown()
        {
            return await _context.Aeronave
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.Modelo} ({a.Tipo})"
                }).ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>> GetAeroportosDropdown()
        {
            return await _context.Aeroporto
                .Select(ap => new SelectListItem
                {
                    Value = ap.Id.ToString(),
                    Text = $"{ap.NomeAeroporto} - {ap.CidadeAeroporto}/{ap.EstadoAeroporto}"
                }).ToListAsync();
        }

        private async Task GerarVooPoltronas(Guid idVoo, Guid idAeronave)
        {
            var assentosDaAeronave = await _context.Assento
                .Where(a => a.IdAeronave == idAeronave)
                .AsNoTracking() 
                .ToListAsync();

            foreach (var assento in assentosDaAeronave)
            {
                var vooPoltrona = new VooPoltrona
                {
                    Id = Guid.NewGuid(),
                    IdVoo = idVoo,
                    IdPoltrona = assento.Id,
                    Status = StatusPoltrona.Disponível
                };
                _context.VooPoltrona.Add(vooPoltrona);
            }
        }

        private bool VooExists(Guid id)
        {
            return _context.Voo.Any(e => e.Id == id);
        }
    }
}