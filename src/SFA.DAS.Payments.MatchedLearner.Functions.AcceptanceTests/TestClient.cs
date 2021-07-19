using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SFA.DAS.Payments.MatchedLearner.Types;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests
{
    public class TestClient
    {
        private static HttpClient _client = new HttpClient();
        private readonly string _url;

        public TestClient()
        {
            _url = TestConfiguration.ApplicationSettings.TargetUrl;

            if (!string.IsNullOrEmpty(_url)) return;

            var factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"EnvironmentName", "Development"},
                    });
                });
            });

            _client = factory.CreateClient();

            _url = _client.BaseAddress.ToString();
        }

        public async Task<MatchedLearnerDto> Handle(long ukprn, long uln)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _url + $"api/v1/{ukprn}/{uln}");

            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{(int)response.StatusCode}");

            var responseAsString = await response.Content.ReadAsStringAsync();
            var responseAsObjectGraph = JsonConvert.DeserializeObject<MatchedLearnerDto>(responseAsString);

            return responseAsObjectGraph;
        }
    }
}
