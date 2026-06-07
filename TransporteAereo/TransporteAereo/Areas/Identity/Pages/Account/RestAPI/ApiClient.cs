using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace TransporteAereo.Areas.Identity.Pages.Account.RestAPI
{
    public class ApiClient
    {

        //Inicializando variável a classe de solicitação e recebimento de respostas HTTP
        private static readonly HttpClient _httpClient = new HttpClient();

        //Método para um GET HTTP de um endpoint
        public Task<string> RequisitaAPIAsync(string endPoint)
        {
            return RequisitaAPIAsync(endPoint, HttpMethod.Get);
        }

        public async Task<string> RequisitaAPIAsync(string endPoint, HttpMethod verbo)
        {
            //Variável para segurar em uma string a resposta obtida sendo um dado ou erro JSON
            string ret = string.Empty;

            //Cria a requisição com o endpoint e verbo específico
            using var request = new HttpRequestMessage(verbo, endPoint);

            try
            {
                //Método assíncrono para mandar a requisição
                using HttpResponseMessage response = await _httpClient.SendAsync(request);

                //Verifica se o código de status HTTP é sucesso
                if(response.IsSuccessStatusCode)
                {
                    ret = await response.Content.ReadAsStringAsync();
                }

                //Se tiver tido erros na requisição esse bloco executa
                else
                {
                    string errorMessage = $"ERRO HTTP: {(int)response.StatusCode} - {response.ReasonPhrase}";
                    ret = $"{{\"errorMessages\":[\"{errorMessage}\"],\"errors\":{{}}}}";
                }

            }

            //Vai tratar erros de rede ou no cliente, nada no endpoint
            catch (HttpRequestException ex)
            {
                ret = $"{{\"errorMessages\":[\"{ex.Message}\"],\"errors\":{{}}}}";
            }

            //Tratamento de mais erros
            catch(Exception ex)
            {
                ret = $"{{\"errorMessages\":[\"{ex.Message}\"],\"errors\":{{}}}}";
            }

            return ret;
        }
    }
}
