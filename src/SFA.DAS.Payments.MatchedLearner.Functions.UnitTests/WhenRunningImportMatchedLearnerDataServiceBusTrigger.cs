using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Functions.UnitTests
{
    public class WhenRunningImportMatchedLearnerDataServiceBusTrigger
    {
        private Mock<ILogger<ImportMatchedLearnerDataHandler>> _mockLogger;
        private Mock<IMatchedLearnerDataImporter> _mockMatchedLearnerDataImporter;
        private ImportMatchedLearnerDataHandler _sut;
        private ImportMatchedLearnerData _submissionSucceededEvent;
        private string _message;
        private TestableMessageHandlerContext _testableMessageHandlerContext;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ImportMatchedLearnerDataHandler>>();
            _mockMatchedLearnerDataImporter = new Mock<IMatchedLearnerDataImporter>();
            _sut = new ImportMatchedLearnerDataHandler(_mockMatchedLearnerDataImporter.Object, _mockLogger.Object);
            _testableMessageHandlerContext = new TestableMessageHandlerContext();

            _submissionSucceededEvent = new ImportMatchedLearnerData
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
            await _sut.Handle(_submissionSucceededEvent, _testableMessageHandlerContext);

            _mockMatchedLearnerDataImporter.Verify(x => x.Import(It.Is<ImportMatchedLearnerData>(
                messages =>
                    messages.JobId == _submissionSucceededEvent.JobId &&
                    messages.Ukprn == _submissionSucceededEvent.Ukprn &&
                    messages.CollectionPeriod == _submissionSucceededEvent.CollectionPeriod &&
                    messages.AcademicYear == _submissionSucceededEvent.AcademicYear)));
        }
    }
}