using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Application
{
    public interface ISubmissionSucceededDelayedImportService
    {
        Task ProcessSubmissionSucceeded(SubmissionJobSucceeded submissionSucceededEvent);
    }

    public class SubmissionSucceededDelayedImportService : ISubmissionSucceededDelayedImportService
    {
        private readonly ApplicationSettings _applicationSettings;
        private readonly IEndpointInstance _endpointInstance;
        private readonly ILogger<SubmissionSucceededDelayedImportService> _logger;

        public SubmissionSucceededDelayedImportService(ApplicationSettings applicationSettings, IEndpointInstance endpointInstance, ILogger<SubmissionSucceededDelayedImportService> logger)
        {
            _applicationSettings = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));
            _endpointInstance = endpointInstance ?? throw new ArgumentNullException(nameof(endpointInstance));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ProcessSubmissionSucceeded(SubmissionJobSucceeded submissionSucceededEvent)
        {
            var delay = TimeSpan.Parse(_applicationSettings.MatchedLearnerImportDelay);

            _logger.LogDebug($"Delaying MatchedLearner Data Import for {delay.TotalSeconds} seconds for job: {submissionSucceededEvent.JobId}");

            var options = new SendOptions();
            options.SetDestination(_applicationSettings.MatchedLearnerImportQueue);
            options.DelayDeliveryWith(delay);

            await _endpointInstance.Send(new ImportMatchedLearnerData
            {
                CollectionPeriod = submissionSucceededEvent.CollectionPeriod,
                JobId = submissionSucceededEvent.JobId,
                Ukprn = submissionSucceededEvent.Ukprn,
                AcademicYear = submissionSucceededEvent.AcademicYear,
                IlrSubmissionDateTime = submissionSucceededEvent.IlrSubmissionDateTime
            }, options).ConfigureAwait(false);

            _logger.LogInformation($"Delayed MatchedLearner Data Import for {delay.TotalSeconds} seconds for job: {submissionSucceededEvent.JobId}");
        }
    }
}