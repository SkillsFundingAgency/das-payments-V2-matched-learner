using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.ServiceTests.MatchedLearnerDataImportServiceTests
{
    [TestFixture]
    public class WhenImportingForFirstCollectionPeriod
    {
        private long _ukprn;
        private byte _collectionPeriod;
        private short _academicYear;
        private Mock<IMatchedLearnerRepository> _mockMatchedLearnerRepository;
        private Mock<IPaymentsRepository> _mockPaymentsRepository;
        private MatchedLearnerDataImportService _sut;

        [SetUp]
        public async Task SetUp()
        {
            _ukprn = 173658;
            _collectionPeriod = 1;
            _academicYear = 2021;
            _mockMatchedLearnerRepository = new Mock<IMatchedLearnerRepository>();
            _mockPaymentsRepository = new Mock<IPaymentsRepository>();

            _mockPaymentsRepository.Setup(x => x.GetDataLockEvents(_ukprn, _academicYear, _collectionPeriod))
                .ReturnsAsync(new List<DataLockEventModel>());

            _mockPaymentsRepository.Setup(x => x.GetApprenticeships(It.IsAny<List<long>>()))
                .ReturnsAsync(new List<ApprenticeshipModel>());

            _sut = new MatchedLearnerDataImportService(_mockMatchedLearnerRepository.Object, _mockPaymentsRepository.Object);

            await _sut.Import(_ukprn, _collectionPeriod, _academicYear);
        }

        [Test]
        public void ThenOnlyRemovesPreviousSubmissionDataForCurrentPeriod()
        {
            _mockMatchedLearnerRepository.Verify(x => x.RemovePreviousSubmissionsData(_ukprn, _academicYear, It.Is<IList<byte>>(y => y.Count == 1 && y.Contains(_collectionPeriod))));
        }
    }
}