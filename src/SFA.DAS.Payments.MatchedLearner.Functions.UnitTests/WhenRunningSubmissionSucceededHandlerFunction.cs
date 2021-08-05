using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.Monitoring.SubmissionJobs.Messages;

namespace SFA.DAS.Payments.MatchedLearner.Functions.UnitTests
{
    public class WhenRunningSubmissionSucceededHandlerFunction
    {
        private Mock<IMatchedLearnerDataImportService> _mockMatchedLearnerDataImportService;
        private Mock<ILogger<SubmissionSucceededHandlerFunction>> _mockLogger;
        private SubmissionSucceededHandlerFunction _sut;
        private SubmissionSucceededEvent _submissionSucceededEvent;
        private string _message;

        [SetUp]
        public void Setup()
        {
            _mockMatchedLearnerDataImportService = new Mock<IMatchedLearnerDataImportService>();
            _mockLogger = new Mock<ILogger<SubmissionSucceededHandlerFunction>>();
            _sut = new SubmissionSucceededHandlerFunction(_mockMatchedLearnerDataImportService.Object, _mockLogger.Object);

            _submissionSucceededEvent = new SubmissionSucceededEvent
            {
                Ukprn = 27367481,
                CollectionPeriod = 1,
                AcademicYear = 2021
            };

            _message = JsonConvert.SerializeObject(_submissionSucceededEvent);
        }

        [Test]
        public async Task ThenMatchedDataIsImported()
        {
            await _sut.Run(_message);
            _mockMatchedLearnerDataImportService.Verify(x => x.Import(_submissionSucceededEvent.Ukprn, _submissionSucceededEvent.CollectionPeriod, _submissionSucceededEvent.AcademicYear));
        }
    }
}