using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SFA.DAS.Payments.MatchedLearner.Api;
using SFA.DAS.Payments.MatchedLearner.Types;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests
{
    public class TestClient
    {
        private static HttpClient _client = new HttpClient();
        private readonly string _url;

        public TestClient()
        {
            _url = TestConfiguration.MatchedLearnerApiConfiguration.TargetUrl;

            if (!string.IsNullOrEmpty(_url)) return;

            _client = new WebApplicationFactory<Startup>().CreateClient();
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
