using System;
using System.Net.Http;
using System.Threading.Tasks;
using MatchedLearnerApi.Types;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MatchedLearnerApi.AcceptanceTests
{
    public class TestClient
    {
        private static HttpClient _client = new HttpClient();
        private readonly string _url;

        public TestClient()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddEnvironmentVariables();
            configurationBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("appsettings.json");

            var configuration = configurationBuilder.Build();

            _url = configuration["TargetUrl"];

            if (string.IsNullOrEmpty(_url))
                _client = new WebApplicationFactory<Startup>().CreateClient();
        }

        public async Task<MatchedLearnerResultDto> Handle(long ukprn, long uln)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _url + $"/api/v1/{ukprn}/{uln}");
            
            var response = await _client.SendAsync(request);

            if(!response.IsSuccessStatusCode)
                throw new Exception($"{(int)response.StatusCode}");

            var responseAsString = await response.Content.ReadAsStringAsync();
            var responseAsObjectGraph = JsonConvert.DeserializeObject<MatchedLearnerResultDto>(responseAsString);

            return responseAsObjectGraph;
        }
    }
}
