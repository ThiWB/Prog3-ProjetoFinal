using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TransporteAereo.Data;
using TransporteAereo.Models;

namespace TransporteAereo.Controllers.Admin
{
    public class AdminAeroportosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminAeroportosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Aeroporto.ToListAsync());
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aeroporto = await _context.Aeroporto
                .FirstOrDefaultAsync(m => m.Id == id);

            if (aeroporto == null)
            {
                return NotFound();
            }

            return View(aeroporto);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("NomeAeroporto,CidadeAeroporto,EstadoAeroporto,Pais")] Aeroporto aeroporto)
        {
            if (ModelState.IsValid)
            {
                aeroporto.Id = Guid.NewGuid();
                _context.Add(aeroporto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(aeroporto);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aeroporto = await _context.Aeroporto.FindAsync(id);
            if (aeroporto == null)
            {
                return NotFound();
            }
            return View(aeroporto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,NomeAeroporto,CidadeAeroporto,EstadoAeroporto,Pais")] Aeroporto aeroporto)
        {
            if (id != aeroporto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aeroporto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AeroportoExists(aeroporto.Id))
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
            return View(aeroporto);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aeroporto = await _context.Aeroporto
                .FirstOrDefaultAsync(m => m.Id == id);

            if (aeroporto == null)
            {
                return NotFound();
            }

            return View(aeroporto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var aeroporto = await _context.Aeroporto.FindAsync(id);
            if (aeroporto != null)
            {
                _context.Aeroporto.Remove(aeroporto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AeroportoExists(Guid id)
        {
            return _context.Aeroporto.Any(e => e.Id == id);
        }
    }
}