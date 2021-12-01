using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using SFA.DAS.Http;
using SFA.DAS.Http.Configuration;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure;
using SFA.DAS.Payments.MatchedLearner.Api;
using SFA.DAS.Payments.MatchedLearner.Types;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests
{
    public class TestClient
    {
        private static HttpClient _client = new HttpClient();
        private readonly string _url;

        public TestClient(bool useV1Api)
        {
            _url = TestConfiguration.TestAzureAdClientSettings.ApiBaseUrl;
            if (!string.IsNullOrEmpty(_url) && !useV1Api)
            {
                _client = new HttpClientBuilder()
                    .WithDefaultHeaders()
                    .WithBearerAuthorisationHeader(new GenerateBearerToken(TestConfiguration.TestAzureAdClientSettings))
                    .Build();

                if (!_url.EndsWith("/")) _url += "/";

                _client.BaseAddress = new Uri(_url);

                return;
            }

            var factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                var path = Path.GetDirectoryName(typeof(Startup).Assembly.Location);
                builder.UseContentRoot(path);
                builder.ConfigureAppConfiguration((context, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "EnvironmentName", "Development" },
                        { "UseV1Api", useV1Api ? "True" : "False" },
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

    public class GenerateBearerToken : IGenerateBearerToken
    {
        private readonly IAzureActiveDirectoryClientConfiguration _configuration;

        public GenerateBearerToken(IAzureActiveDirectoryClientConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> Generate()
        {
            var authority = $"https://login.microsoftonline.com/{TestConfiguration.TestAzureAdClientSettings.Tenant}";
            var clientCredential = new ClientCredential(_configuration.ClientId, _configuration.ClientSecret);
            var context = new AuthenticationContext(authority, true);
            var result = await context.AcquireTokenAsync(_configuration.IdentifierUri, clientCredential);

            return result.AccessToken;
        }
    }
}
