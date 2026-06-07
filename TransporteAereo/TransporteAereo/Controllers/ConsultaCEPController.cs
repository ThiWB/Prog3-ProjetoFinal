using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using TransporteAereo.Areas.Identity.Pages.Account.RestAPI;

namespace TransporteAereo.Controllers
{
    [Route("api/ConsultaCEP")]
    [ApiController]
    public class ConsultaCEPController : Controller
    {
        private readonly ApiClient _apiClient = new ApiClient();

        [HttpGet(Name = "Consulta")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CEP>> Consulta(string Codigo)
        {
            string endPoint = "https://viacep.com.br/ws/" + Codigo + "/json";

            string responseValue = await _apiClient.RequisitaAPIAsync(endPoint);

            if (responseValue.Contains("errorMessages"))
            {
                return BadRequest(responseValue);
            }

            ViaCEP dto = JsonSerializer.Deserialize<ViaCEP>(responseValue);

            if(dto.cep == null)
            {
                return NotFound();
            }

            CEP ret = new CEP();
            ret.Codigo = dto.cep;
            ret.Logradouro = dto.logradouro;
            ret.Bairro = dto.bairro;
            ret.Cidade = dto.localidade;
            ret.Estado = dto.estado;
            return Ok(ret);
        }
    }
}
