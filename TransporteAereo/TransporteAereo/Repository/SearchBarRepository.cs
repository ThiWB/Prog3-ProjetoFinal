using TransporteAereo.Data;
using TransporteAereo.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; 
using System.Linq;
using System.Threading.Tasks;

namespace TransporteAereo.Repository
{
    public class SearchBarRepository : ISearchBarRepository
    {
        private readonly ApplicationDbContext _context;

        public SearchBarRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Viagem>> Search(string searchString)
        {
            var query = _context.Viagem
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Voos)
                    .ThenInclude(v => v.Aeronave)
                .AsQueryable(); 

            if (!string.IsNullOrEmpty(searchString))
            {
                string termo = searchString.ToLower();

                query = query.Where(v =>
                    v.AeroportoOrigem.NomeAeroporto.ToLower().Contains(termo) ||
                    v.AeroportoDestino.NomeAeroporto.ToLower().Contains(termo) ||
                    v.AeroportoOrigem.CidadeAeroporto.ToLower().Contains(termo) ||
                    v.AeroportoDestino.CidadeAeroporto.ToLower().Contains(termo)
                );
            }
            return await query.ToListAsync();
        }
    }
}