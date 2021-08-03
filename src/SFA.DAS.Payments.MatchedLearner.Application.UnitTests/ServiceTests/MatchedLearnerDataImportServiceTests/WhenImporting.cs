using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.ServiceTests.MatchedLearnerDataImportServiceTests
{
    [TestFixture]
    public class WhenImporting
    {
        private long _ukprn;
        private byte _collectionPeriod;
        private short _academicYear;
        private Mock<IMatchedLearnerRepository> _mockMatchedLearnerRepository;
        private Mock<IPaymentsRepository> _mockPaymentsRepository;
        private MatchedLearnerDataImportService _sut;
        private List<DataLockEventModel> _dataLockEvents;
        private List<ApprenticeshipModel> _apprenticeships;
        private Guid _dataLockEventId;

        [SetUp]
        public async Task SetUp()
        {
            _ukprn = 173658;
            _collectionPeriod = 5;
            _academicYear = 2021;
            _mockMatchedLearnerRepository = new Mock<IMatchedLearnerRepository>();
            _mockPaymentsRepository = new Mock<IPaymentsRepository>();

            _dataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    EventId = _dataLockEventId,
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>
                    {
                        new DataLockEventPayablePeriodModel
                        {
                            ApprenticeshipId = 112
                        }
                    },
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>
                    {
                        new DataLockEventNonPayablePeriodModel
                        {
                            Failures = new List<DataLockEventNonPayablePeriodFailureModel>
                            {
                                new DataLockEventNonPayablePeriodFailureModel
                                {
                                    ApprenticeshipId = 114
                                }
                            }
                        }
                    }
                }
            };

            _apprenticeships = new List<ApprenticeshipModel>
            {
                new ApprenticeshipModel
                {
                    Id = 112,
                    Uln = 1000112
                },
                new ApprenticeshipModel
                {
                    Id = 114,
                    Uln = 1000114
                }
            };

            _mockPaymentsRepository.Setup(x => x.GetDataLockEvents(_ukprn, _academicYear, _collectionPeriod))
                .ReturnsAsync(_dataLockEvents);

            _mockPaymentsRepository.Setup(x => x.GetApprenticeships(It.IsAny<List<long>>()))
                .ReturnsAsync(_apprenticeships);

            _sut = new MatchedLearnerDataImportService(_mockMatchedLearnerRepository.Object, _mockPaymentsRepository.Object);

            await _sut.Import(_ukprn, _collectionPeriod, _academicYear);
        }

        [Test]
        public void ThenRemovesPreviousSubmissionDataForCurrentPeriod()
        {
            _mockMatchedLearnerRepository.Verify(x => x.RemovePreviousSubmissionsData(_ukprn, _academicYear, It.Is<IList<byte>>(y => y.Contains(_collectionPeriod))));
        }

        [Test]
        public void ThenRemovesPreviousSubmissionDataForPreviousPeriod()
        {
            _mockMatchedLearnerRepository.Verify(x => x.RemovePreviousSubmissionsData(_ukprn, _academicYear, It.Is<IList<byte>>(y => y.Contains((byte)(_collectionPeriod - 1)))));
        }

        [Test]
        public void ThenRemovesPreviousApprenticeshipData()
        {
            _mockMatchedLearnerRepository.Verify(x => x.RemoveApprenticeships(It.Is<List<long>>(
                y => y.Count == 2 
                     && y.Contains(112) 
                     && y.Contains(114))));
        }

        [Test]
        public void ThenStoresNewApprenticeshipData()
        {
            _mockMatchedLearnerRepository.Verify(x => x.StoreApprenticeships(It.Is<List<ApprenticeshipModel>>(
                y => y.Count == 2
                && y.Any(z => z.Id == 112 && z.Uln == 1000112)
                && y.Any(z => z.Id == 114 && z.Uln == 1000114)
                ), It.IsAny<CancellationToken>()));
        }

        [Test]
        public void ThenStoresDataLocks()
        {
            _mockMatchedLearnerRepository.Verify(x => x.StoreDataLocks(It.Is<List<DataLockEventModel>>(
                y => y.Count == 1
                     && y.Any(z => z.EventId == _dataLockEventId)
            ), It.IsAny<CancellationToken>()));
        }
    }
}
