using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace RFQVerifiedSchedular
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
                /* local 
                string endpoint = "http://localhost:16115/api/MSDEnquiry/VerifyRFQ";*/
                #endregion
                #region
                /* Prod Env
                string endpoint = "http://192.168.26.20:81/api/MSDEnquiry/VerifyRFQ";*/
                #endregion
                #region
                /*UAT Env*/
                string endpoint = "http://192.168.26.23:81/api/MSDEnquiry/VerifyRFQ";
                #endregion
                #region input request
                var data = new
                    {
                        errorRFQ = "YES"
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
                }
            #endregion
        }
    }
}
