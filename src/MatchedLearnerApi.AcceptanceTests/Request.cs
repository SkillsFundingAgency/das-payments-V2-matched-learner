using System;
using System.Net.Http;
using System.Threading.Tasks;
using MatchedLearnerApi.Types;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MatchedLearnerApi.AcceptanceTests
{
    public class Request
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly string _url;

        public Request()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddEnvironmentVariables();
            configurationBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("appsettings.json");

            var configuration = configurationBuilder.Build();

            _url = configuration["TargetUrl"];
        }

        public async Task<MatchedLearnerResultDto> Handle(long ukprn, long uln)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _url + $"/api/v1/{ukprn}/{uln}");
            
            try
            {
                var response = await Client.SendAsync(request);

                if(!response.IsSuccessStatusCode)
                    throw new Exception($"{(int)response.StatusCode}");

                var responseAsString = await response.Content.ReadAsStringAsync();
                var responseAsObjectGraph = JsonConvert.DeserializeObject<MatchedLearnerResultDto>(responseAsString);

                return responseAsObjectGraph;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
