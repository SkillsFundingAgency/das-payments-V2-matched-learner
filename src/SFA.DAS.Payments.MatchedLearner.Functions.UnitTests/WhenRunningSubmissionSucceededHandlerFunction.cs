using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Functions.UnitTests
{
    public class WhenRunningSubmissionSucceededHandlerFunction
    {
        private Mock<ILogger<SubmissionSucceededServiceBusTrigger>> _mockLogger;
        private Mock<ISubmissionSucceededDelayedImportService> _mockSubmissionSucceededDelayedImportService;
        private TestableEndpointInstance _mockFunctionEndpoint;
        private SubmissionSucceededServiceBusTrigger _sut;
        private SubmissionJobSucceeded _submissionSucceededEvent;
        private string _message;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<SubmissionSucceededServiceBusTrigger>>();
            _mockSubmissionSucceededDelayedImportService = new Mock<ISubmissionSucceededDelayedImportService>();
            _mockFunctionEndpoint = new TestableEndpointInstance();

            _sut = new SubmissionSucceededServiceBusTrigger(_mockSubmissionSucceededDelayedImportService.Object, _mockLogger.Object);

            _submissionSucceededEvent = new SubmissionJobSucceeded
            {
                Ukprn = 27367481,
                CollectionPeriod = 1,
                AcademicYear = 2021
            };

            _message = JsonConvert.SerializeObject(_submissionSucceededEvent);
        }

        [Test]
        public async Task ThenImportMatchedLearnerDataMessageIsSent()
        {
            await _sut.RunServiceBusTrigger(_message);

            _mockSubmissionSucceededDelayedImportService.Verify(x => x.ProcessSubmissionSucceeded(It.Is<SubmissionJobSucceeded>(
                messages =>
                    messages.JobId == _submissionSucceededEvent.JobId &&
                    messages.Ukprn == _submissionSucceededEvent.Ukprn &&
                    messages.CollectionPeriod == _submissionSucceededEvent.CollectionPeriod &&
                    messages.AcademicYear == _submissionSucceededEvent.AcademicYear)));
        }
    }
}