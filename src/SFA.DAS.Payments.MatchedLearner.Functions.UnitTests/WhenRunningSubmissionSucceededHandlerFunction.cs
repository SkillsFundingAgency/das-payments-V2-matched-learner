using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Functions.UnitTests
{
    public class WhenRunningSubmissionSucceededHandlerFunction
    {
        private Mock<IMatchedLearnerDataImporter> _mockMatchedLearnerDataImporter;
        private Mock<ILogger<SubmissionSucceededHandlerFunction>> _mockLogger;
        private SubmissionSucceededHandlerFunction _sut;
        private SubmissionJobSucceeded _submissionSucceededEvent;
        private string _message;

        [SetUp]
        public void Setup()
        {
            _mockMatchedLearnerDataImporter = new Mock<IMatchedLearnerDataImporter>();
            _mockLogger = new Mock<ILogger<SubmissionSucceededHandlerFunction>>();
            _sut = new SubmissionSucceededHandlerFunction(_mockMatchedLearnerDataImporter.Object, _mockLogger.Object);

            _submissionSucceededEvent = new SubmissionJobSucceeded
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
            _mockMatchedLearnerDataImporter.Verify(x => x.Import(It.Is<SubmissionJobSucceeded>(
                messages =>  
                    messages.JobId == _submissionSucceededEvent.JobId && 
                    messages.Ukprn == _submissionSucceededEvent.Ukprn &&
                    messages.CollectionPeriod == _submissionSucceededEvent.CollectionPeriod &&
                    messages.AcademicYear == _submissionSucceededEvent.AcademicYear)));
        }
    }
}