using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Functions.UnitTests
{
    public class WhenRunningSubmissionSucceededHandlerFunction
    {
        private Mock<ILogger<SubmissionSucceededHandler>> _mockLogger;
        private Mock<ISubmissionSucceededDelayedImportService> _mockSubmissionSucceededDelayedImportService;
        private TestableMessageHandlerContext _testableMessageHandlerContext;
        private SubmissionSucceededHandler _sut;
        private SubmissionJobSucceeded _submissionSucceededEvent;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<SubmissionSucceededHandler>>();
            _mockSubmissionSucceededDelayedImportService = new Mock<ISubmissionSucceededDelayedImportService>();
            _testableMessageHandlerContext = new TestableMessageHandlerContext();

            _sut = new SubmissionSucceededHandler(_mockSubmissionSucceededDelayedImportService.Object, _mockLogger.Object);

            _submissionSucceededEvent = new SubmissionJobSucceeded
            {
                Ukprn = 27367481,
                CollectionPeriod = 1,
                AcademicYear = 2021
            };
        }

        [Test]
        public async Task ThenImportMatchedLearnerDataMessageIsSent()
        {
            await _sut.Handle(_submissionSucceededEvent, _testableMessageHandlerContext);

            _mockSubmissionSucceededDelayedImportService.Verify(x => x.ProcessSubmissionSucceeded(It.Is<SubmissionJobSucceeded>(
                messages =>
                    messages.JobId == _submissionSucceededEvent.JobId &&
                    messages.Ukprn == _submissionSucceededEvent.Ukprn &&
                    messages.CollectionPeriod == _submissionSucceededEvent.CollectionPeriod &&
                    messages.AcademicYear == _submissionSucceededEvent.AcademicYear), It.IsAny<IMessageHandlerContext>()));
        }
    }
}