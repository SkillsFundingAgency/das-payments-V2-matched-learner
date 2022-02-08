using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Functions.Migration;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Application
{
    public interface ISubmissionSucceededDelayedImportService
    {
        Task ProcessSubmissionSucceeded(SubmissionJobSucceeded submissionSucceededEvent, IMessageHandlerContext messageHandlerContext);
    }

    public class SubmissionSucceededDelayedImportService : ISubmissionSucceededDelayedImportService
    {
        private readonly ApplicationSettings _applicationSettings;
        private readonly ILogger<SubmissionSucceededDelayedImportService> _logger;

        public SubmissionSucceededDelayedImportService(ApplicationSettings applicationSettings, ILogger<SubmissionSucceededDelayedImportService> logger)
        {
            _applicationSettings = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ProcessSubmissionSucceeded(SubmissionJobSucceeded submissionSucceededEvent, IMessageHandlerContext messageHandlerContext)
        {
            var delay = TimeSpan.Parse(_applicationSettings.MatchedLearnerImportDelay);

            _logger.LogDebug($"Delaying MatchedLearner Data Import for {delay.TotalSeconds} seconds for job: {submissionSucceededEvent.JobId}");

            var options = SendLocally.Options;
            options.DelayDeliveryWith(delay);

            await messageHandlerContext.Send(new ImportMatchedLearnerData
            {
                CollectionPeriod = submissionSucceededEvent.CollectionPeriod,
                JobId = submissionSucceededEvent.JobId,
                Ukprn = submissionSucceededEvent.Ukprn,
                AcademicYear = submissionSucceededEvent.AcademicYear,
                IlrSubmissionDateTime = submissionSucceededEvent.IlrSubmissionDateTime,
                EventTime = submissionSucceededEvent.EventTime
            }, options).ConfigureAwait(false);

            _logger.LogInformation($"Delayed MatchedLearner Data Import for {delay.TotalSeconds} seconds for job: {submissionSucceededEvent.JobId}");
        }
    }
}