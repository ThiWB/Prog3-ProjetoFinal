using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TransporteAereo.Data;
using TransporteAereo.Models;
using TransporteAereo.ViewModels.Admin;

namespace TransporteAereo.Controllers.Admin
{
    public class AdminViagensController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminViagensController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Viagem.Include(v => v.AeroportoDestino).Include(v => v.AeroportoOrigem);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viagem = await _context.Viagem
                .Include(v => v.AeroportoDestino)
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.Voos)
                    .ThenInclude(voo => voo.AeroportoOrigem)
                .Include(v => v.Voos)
                    .ThenInclude(voo => voo.AeroportoDestino)
                .Include(v => v.Voos)
                    .ThenInclude(voo => voo.Aeronave)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (viagem == null)
            {
                return NotFound();
            }

            return View(viagem);
        }

        public async Task<IActionResult> Create()
        {
            var model = new ViagemCreateViewModel
            {
                VoosDisponiveis = await GetVoosSelectList(Guid.Empty) 
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ViagemCreateViewModel model)
        {
            if (model.IdsVoosSelecionados == null || !model.IdsVoosSelecionados.Any())
            {
                ModelState.AddModelError(nameof(model.IdsVoosSelecionados), "Selecione pelo menos um voo para criar a viagem.");
            }

            if (!ModelState.IsValid)
            {
                model.VoosDisponiveis = await GetVoosSelectList(Guid.Empty);
                return View(model);
            }

            var voos = await _context.Voo
                .Where(v => model.IdsVoosSelecionados.Contains(v.Id))
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .ToListAsync();

            var voosOrdenados = model.IdsVoosSelecionados
                .Select(id => voos.FirstOrDefault(v => v.Id == id))
                .Where(v => v != null)
                .ToList();

            if (!ValidaSequenciaVoos(voosOrdenados))
            {
                model.VoosDisponiveis = await GetVoosSelectList(Guid.Empty);
                return View(model);
            }

            var primeiroVoo = voosOrdenados.First();
            var ultimoVoo = voosOrdenados.Last();
            var precoTotal = voosOrdenados.Sum(v => v.PrecoBase);

            var novaViagem = new Viagem
            {
                Id = Guid.NewGuid(),
                NomeViagem = model.NomeViagem,
                IdOrigem = primeiroVoo.IdOrigem,
                IdDestino = ultimoVoo.IdDestino,
                DataPartida = primeiroVoo.HorarioSaida,
                DataChegada = ultimoVoo.HorarioChegada,
                PrecoTotal = precoTotal,
            };

            _context.Viagem.Add(novaViagem);

            foreach (var voo in voosOrdenados)
            {
                voo.IdViagem = novaViagem.Id;
                _context.Voo.Update(voo);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viagem = await _context.Viagem
                .Include(v => v.Voos.OrderBy(v => v.HorarioSaida)) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (viagem == null)
            {
                return NotFound();
            }

            var model = new ViagemEditViewModel
            {
                Id = viagem.Id,
                NomeViagem = viagem.NomeViagem,
                IdsVoosSelecionados = viagem.Voos.Select(v => v.Id).ToList(),
            };

            model.VoosDisponiveis = await GetVoosSelectList(viagem.Id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ViagemEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (model.IdsVoosSelecionados == null || !model.IdsVoosSelecionados.Any())
            {
                ModelState.AddModelError(nameof(model.IdsVoosSelecionados), "Selecione pelo menos um voo para manter a viagem.");
            }

            if (!ModelState.IsValid)
            {
                model.VoosDisponiveis = await GetVoosSelectList(model.Id);
                return View(model);
            }

            var voosAtuais = await _context.Voo
                .Where(v => model.IdsVoosSelecionados.Contains(v.Id))
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .ToListAsync();

            var voosOrdenados = model.IdsVoosSelecionados
                .Select(vooId => voosAtuais.FirstOrDefault(v => v.Id == vooId))
                .Where(v => v != null)
                .ToList();

            if (!ValidaSequenciaVoos(voosOrdenados))
            {
                model.VoosDisponiveis = await GetVoosSelectList(model.Id);
                return View(model);
            }


            var viagemOriginal = await _context.Viagem
                .Include(v => v.Voos)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (viagemOriginal == null)
            {
                return NotFound();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var voosAntigosIds = viagemOriginal.Voos.Select(v => v.Id).ToList();
                    var voosParaDesassociar = viagemOriginal.Voos.Where(v => !model.IdsVoosSelecionados.Contains(v.Id)).ToList();

                    foreach (var voo in voosParaDesassociar)
                    {
                        voo.IdViagem = null;
                        _context.Voo.Update(voo);
                    }

                    foreach (var voo in voosOrdenados)
                    {
                        voo.IdViagem = viagemOriginal.Id;
                        _context.Voo.Update(voo); 
                    }

                    var primeiroVoo = voosOrdenados.First();
                    var ultimoVoo = voosOrdenados.Last();
                    var precoTotal = voosOrdenados.Sum(v => v.PrecoBase);

                    viagemOriginal.NomeViagem = model.NomeViagem;
                    viagemOriginal.IdOrigem = primeiroVoo.IdOrigem;
                    viagemOriginal.IdDestino = ultimoVoo.IdDestino;
                    viagemOriginal.DataPartida = primeiroVoo.HorarioSaida;
                    viagemOriginal.DataChegada = ultimoVoo.HorarioChegada;
                    viagemOriginal.PrecoTotal = precoTotal;

                    _context.Viagem.Update(viagemOriginal);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ViagemExists(viagemOriginal.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao salvar as alterações da viagem. Tente novamente.");
                    model.VoosDisponiveis = await GetVoosSelectList(model.Id);
                    return View(model);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viagem = await _context.Viagem
                .Include(v => v.AeroportoDestino)
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.Voos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (viagem == null)
            {
                return NotFound();
            }

            return View(viagem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var viagem = await _context.Viagem
                .Include(v => v.Voos)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (viagem != null)
            {
                foreach (var voo in viagem.Voos)
                {
                    voo.IdViagem = null;
                    _context.Voo.Update(voo);
                }

                _context.Viagem.Remove(viagem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ViagemExists(Guid id)
        {
            return _context.Viagem.Any(e => e.Id == id);
        }

        private bool ValidaSequenciaVoos(List<Voo> voosOrdenados)
        {
            for (int i = 0; i < voosOrdenados.Count - 1; i++)
            {
                var vooAtual = voosOrdenados[i];
                var proximoVoo = voosOrdenados[i + 1];

                if (vooAtual.IdDestino != proximoVoo.IdOrigem)
                {
                    ModelState.AddModelError(string.Empty, $"A conexão entre o voo {i + 1} ({vooAtual.AeroportoDestino.CidadeAeroporto}) e o voo {i + 2} ({proximoVoo.AeroportoOrigem.CidadeAeroporto}) é inválida. O destino do primeiro voo não corresponde à origem do segundo.");
                    return false;
                }

                if (proximoVoo.HorarioSaida < vooAtual.HorarioChegada)
                {
                    ModelState.AddModelError(string.Empty, $"O voo {i + 2} (Partida: {proximoVoo.HorarioSaida:dd/MM HH:mm}) parte antes da chegada do voo {i + 1} (Chegada: {vooAtual.HorarioChegada:dd/MM HH:mm}). Horários de conexão inválidos.");
                    return false;
                }
            }
            return true;
        }

        /// <param name="viagemId">O ID da viagem que está sendo criada/editada. Use Guid.Empty para criação.</param>
        private async Task<IEnumerable<SelectListItem>> GetVoosSelectList(Guid viagemId)
        {
            var voosDisponiveis = await _context.Voo
                .Where(v => v.IdViagem == null || v.IdViagem == viagemId)
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .OrderBy(v => v.HorarioSaida)
                .ToListAsync();

            return voosDisponiveis.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.AeroportoOrigem.CidadeAeroporto} ({v.HorarioSaida:dd/MM HH:mm}) → " +
                       $"{v.AeroportoDestino.CidadeAeroporto} ({v.HorarioChegada:dd/MM HH:mm}) | " +
                       $"Base: {v.PrecoBase:C}",
                Selected = v.IdViagem == viagemId && viagemId != Guid.Empty
            });
        }
    }
}