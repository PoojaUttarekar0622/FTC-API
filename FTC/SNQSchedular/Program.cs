using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace SNQSchedular
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
        }
        static async Task RunAsync()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (HttpClient client = new HttpClient(clientHandler))
            {
                #region
                /* Prod Env
                string endpoint = "http://192.168.26.20:81/api/Report/GetSNQDaily_Report";*/
                #endregion
                #region
                /*UAT Env*/
                string endpoint = "http://192.168.26.23:81/api/Report/GetSNQDaily_Report";
                #endregion
                #region
                /*for MESPAS
                 string endpoint = "http://192.168.26.23:83/api/Report/GetSNQDaily_Report";*/
                #endregion
                #region input request
                var data = new
                {
                    fromDate = "",
                    toDate = ""
                };
                #endregion
                StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                #region
                using (var Response = await client.PostAsync(endpoint, content))
                {
                    if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var result = Response.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                    }
                }
                #endregion
            }
        }
    }
}
