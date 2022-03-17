using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.ServiceTests.MatchedLearnerDataImportServiceTests
{
    [TestFixture]
    public class WhenImportingForFirstCollectionPeriod
    {
        private ImportMatchedLearnerData _importMatchedLearnerData;
        private Mock<IMatchedLearnerRepository> _mockMatchedLearnerRepository;
        private Mock<IPaymentsRepository> _mockPaymentsRepository;
        private MatchedLearnerDataImportService _sut;

        [SetUp]
        public async Task SetUp()
        {
            _importMatchedLearnerData = new ImportMatchedLearnerData
            {
                Ukprn = 173658,
                CollectionPeriod = 1,
                AcademicYear = 2021,
                JobId = 123,
            };
            
            _mockMatchedLearnerRepository = new Mock<IMatchedLearnerRepository>();
            _mockPaymentsRepository = new Mock<IPaymentsRepository>();

            _mockPaymentsRepository.Setup(x => x.GetDataLockEvents(_importMatchedLearnerData))
                .ReturnsAsync(new List<DataLockEventModel>());

            _mockPaymentsRepository.Setup(x => x.GetApprenticeships(It.IsAny<List<long>>()))
                .ReturnsAsync(new List<ApprenticeshipModel>());

            _sut = new MatchedLearnerDataImportService(_mockMatchedLearnerRepository.Object, _mockPaymentsRepository.Object);

            await _sut.Import(_importMatchedLearnerData);
        }

        [Test]
        public void ThenOnlyRemovesPreviousSubmissionDataForCurrentPeriod()
        {
            _mockMatchedLearnerRepository.Verify(x => x.RemovePreviousSubmissionsData(_importMatchedLearnerData.Ukprn, _importMatchedLearnerData.AcademicYear, It.Is<IList<byte>>(y => y.Count == 1 && y.Contains(_importMatchedLearnerData.CollectionPeriod))));
        }
    }
}