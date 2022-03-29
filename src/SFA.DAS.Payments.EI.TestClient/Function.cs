using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Payments.MatchedLearner.Types;

namespace SFA.DAS.Payments.EI.TestClient
{
    public static class Function
    {
        [FunctionName("TestFunction")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var instanceId = await starter.StartNewAsync(nameof(LearnerMatchingOrchestrator));

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }

    public class LearnerMatchingOrchestrator
    {
        [FunctionName(nameof(LearnerMatchingOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var apprenticeshipIncentives = await context.CallActivityAsync<List<ApprenticeshipInput>>(nameof(GetAllApprenticeshipIncentives), null);

            var tasks = apprenticeshipIncentives.Select(incentive => context.CallSubOrchestratorAsync(nameof(LearnerMatchingApprenticeshipOrchestrator), incentive)).ToList();

            await Task.WhenAll(tasks);
        }
    }

    public class GetAllApprenticeshipIncentives
    {
        private readonly TestDataContext _dataContext;

        public GetAllApprenticeshipIncentives(IConfiguration configuration)
        {
            var connectionString = configuration["ConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
                throw new ApplicationException("Configuration is not initialized correctly");

            var options = new DbContextOptionsBuilder()
                .UseSqlServer(new SqlConnection(connectionString), optionsBuilder => optionsBuilder.CommandTimeout(540))
                .Options;

            _dataContext = new TestDataContext(options);
        }

        [FunctionName(nameof(GetAllApprenticeshipIncentives))]
        public async Task<List<ApprenticeshipInput>> Get([ActivityTrigger] object input)
        {
            var response = await _dataContext.ApprenticeshipInput.ToListAsync();

            return response;
        }
    }

    public class LearnerMatchingApprenticeshipOrchestrator
    {
        [FunctionName(nameof(LearnerMatchingApprenticeshipOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var incentive = context.GetInput<ApprenticeshipInput>();

            await context.CallActivityWithRetryAsync(nameof(LearnerMatchAndUpdate), new RetryOptions(TimeSpan.FromSeconds(1), 3), incentive);

            var deadline = context.CurrentUtcDateTime.Add(TimeSpan.FromMilliseconds(100));
            await context.CreateTimer(deadline, CancellationToken.None);

            await context.CallActivityWithRetryAsync(nameof(LearnerMatchAndUpdate2), new RetryOptions(TimeSpan.FromSeconds(1), 3), incentive);
        }
    }


    public class LearnerMatchAndUpdate
    {
        private readonly HttpClient _client;
        private readonly TestDataContext _dataContext;

        public LearnerMatchAndUpdate(IConfiguration configuration, HttpClient client)
        {
            _client = client;

            var connectionString = configuration["ConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
                throw new ApplicationException("Configuration is not initialized correctly");

            var options = new DbContextOptionsBuilder()
                .UseSqlServer(new SqlConnection(connectionString), optionsBuilder => optionsBuilder.CommandTimeout(540))
                .Options;

            _dataContext = new TestDataContext(options);
        }

        [FunctionName(nameof(LearnerMatchAndUpdate))]
        public async Task Create([ActivityTrigger] ApprenticeshipInput apprenticeshipInput)
        {
            var response = await _client.GetAsync($"api/v1/{apprenticeshipInput.Ukprn}/{apprenticeshipInput.Uln}?");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return;
            }

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<MatchedLearnerDto>(jsonString);

            if (data == null || data.Uln != apprenticeshipInput.Uln)
            {
                throw new InvalidOperationException(jsonString);
            }

            var apprenticeshipOutPutResult = new ApprenticeshipOutPut { Ukprn = apprenticeshipInput.Ukprn, Uln = apprenticeshipInput.Uln, LearnerJson = jsonString };
            
            await _dataContext.ApprenticeshipOutPut.AddAsync(apprenticeshipOutPutResult);

            await _dataContext.SaveChangesAsync();

        }
    }
    public class LearnerMatchAndUpdate2
    {
        private readonly HttpClient _client;

        public LearnerMatchAndUpdate2(HttpClient client)
        {
            _client = client;
        }

        [FunctionName(nameof(LearnerMatchAndUpdate2))]
        public async Task<ApprenticeshipOutPut> Create([ActivityTrigger] ApprenticeshipInput apprenticeshipInput)
        {
            var response = await _client.GetAsync($"api/v1/{apprenticeshipInput.Ukprn}/{apprenticeshipInput.Uln}?");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<MatchedLearnerDto>(jsonString);

            if (data == null || data.Uln != apprenticeshipInput.Uln)
            {
                throw new InvalidOperationException(jsonString);
            }

            return new ApprenticeshipOutPut { Ukprn = apprenticeshipInput.Ukprn, Uln = apprenticeshipInput.Uln, LearnerJson = jsonString };
        }
    }
}

