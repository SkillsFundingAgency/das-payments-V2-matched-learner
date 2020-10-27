using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SFA.DAS.Payments.MatchedLearner.Api;
using SFA.DAS.Payments.MatchedLearner.Types;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Environment.SetEnvironmentVariable("APPSETTING_Environment", "Development");

            builder.ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appSettings.json", optional: false, reloadOnChange: false);
                config.AddEnvironmentVariables();
            });
        }
    }

    public class TestClient
    {
        private static HttpClient _client = new HttpClient();
        private readonly string _url;

        public TestClient()
        {
            _url = TestConfiguration.TargetUrl;

            if (!string.IsNullOrEmpty(_url)) return;

            _client = new CustomWebApplicationFactory<Startup>().CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost:44300") });
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
