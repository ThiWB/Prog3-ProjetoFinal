using TransporteAereo.Models;

namespace TransporteAereo.Repository
{
    public interface ISearchBarRepository
    {
        public Task<List<Viagem>> Search(string searchString);
    }
}